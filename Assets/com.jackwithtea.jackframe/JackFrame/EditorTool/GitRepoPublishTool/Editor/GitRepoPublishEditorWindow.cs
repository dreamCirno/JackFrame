using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

namespace JackFrame.EditorTool {

    public class GitRepoPublishEditorWindow : EditorWindow {

        GitRepoConfigModel configModel;
        [SerializeField] PublishSo publishModel;
        int toolbarIndex = 0;

        [MenuItem(MENU_CONTEXT_NAME.L1_TOOL + "/GitPublish")]
        public static void Open() {
            GitRepoPublishEditorWindow window = EditorWindow.GetWindow<GitRepoPublishEditorWindow>();
            window.titleContent.text = "Git发布工具";
            window.Show();
        }

        void OnEnable() {
            if (configModel == null) {
                configModel = ReadDataFile();
            }
            if (publishModel == null) {
                publishModel = ScriptableObject.CreateInstance<PublishSo>();
            }   
        }

        void OnGUI() {

            if (configModel == null) {
                if (GUILayout.Button("创建配置文件")) {
                    CreateDataFile();
                }
                if (GUILayout.Button("重试读取配置文件")) {
                    configModel = ReadDataFile();
                }
                return;
            }

            if (publishModel.currentVersion == null) {
                publishModel.currentVersion = ReadCurrentVersion(configModel);
            }

            toolbarIndex = GUILayout.Toolbar(toolbarIndex, new string[] { "基本配置", "准备发布" });

            if (toolbarIndex == 0) {

                GUILayout.Label("CHANGELOG.md 所在路径");
                configModel.changeLogFilePath = GUILayout.TextField(configModel.changeLogFilePath);

                GUILayout.Space(10);
                GUILayout.Label("VERSION 所在路径");
                configModel.versionFilePath = GUILayout.TextField(configModel.versionFilePath);

                GUILayout.Space(10);
                GUILayout.Label("package.json 所在路径");
                configModel.packageJsonFilePath = GUILayout.TextField(configModel.packageJsonFilePath);

                GUILayout.Space(10);
                if (GUILayout.Button("保存")) {
                    SaveDataFile(configModel);
                }

            } else if (toolbarIndex == 1) {

                GUILayout.Label($"当前版本号: {publishModel.currentVersion}");
                GUILayout.Space(10);

                GUILayout.Label("待发布版本号(例1.1.0, 前后不加符号)");
                publishModel.semanticVersion = GUILayout.TextField(publishModel.semanticVersion);

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Added");
                publishModel.changeLogAdded = GUILayout.TextArea(publishModel.changeLogAdded, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Changed");
                publishModel.changeLogChanged = GUILayout.TextArea(publishModel.changeLogChanged, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Removed");
                publishModel.changeLogRemoved = GUILayout.TextArea(publishModel.changeLogRemoved, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Fixed");
                publishModel.changeLogFixed = GUILayout.TextArea(publishModel.changeLogFixed, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Other");
                publishModel.changeLogOther = GUILayout.TextArea(publishModel.changeLogOther, GUILayout.MinHeight(50));

                if (GUILayout.Button("保存发布")) {
                    string title = "发布确认";
                    string content = $"以下文件将会被修改:\r\n"
                                    + $"  {configModel.changeLogFilePath}\r\n"
                                    + $"  {configModel.versionFilePath}\r\n"
                                    + $"  {configModel.packageJsonFilePath}\r\n"
                                    + $"是否确认保存?";
                    string ok = "确认";
                    string cancel = "取消";
                    if (EditorUtility.DisplayDialog(title, content, ok, cancel)) {
                        SaveChange(configModel, publishModel);
                    }
                } 

            }

        }

        // ==== 保存发布信息到文件 ====
        void SaveChange(GitRepoConfigModel config, PublishSo publishModel) {

            const string NOTICE = "请放置在Assets目录下, 并且路径字符串不需要添加Assets前缀";

            string dir = Application.dataPath;
            string filePath;
            // SAVE VERSION
            //      COVER VERSION
            filePath = Path.Combine(dir, config.versionFilePath);
            if (!File.Exists(filePath)) {
                EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
                return;
            }
            FileHelper.SaveFileText(publishModel.semanticVersion, filePath);

            // class PackageInfo
            // SAVE PACKAGEJSON:
            //      VERSION
            filePath = Path.Combine(dir, config.packageJsonFilePath);
            if (!File.Exists(filePath)) {
                EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
                return;
            }
            string jsonStr = FileHelper.LoadTextFromFile(filePath);
            PackageJsonObj json = JsonConvert.DeserializeObject<PackageJsonObj>(jsonStr);
            json.version = publishModel.semanticVersion;
            jsonStr = JsonConvert.SerializeObject(json, Formatting.Indented);
            FileHelper.SaveFileText(jsonStr, filePath);

            // SAVE CHANGELOG:
            //      VERSION
            //      ADDED/CHANGED/REMOVED/FIXED/OTHER
            filePath = Path.Combine(dir, config.changeLogFilePath);
            if (!File.Exists(filePath)) {
                EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
                return;
            }
            ChangeLogModifier changeLog = new ChangeLogModifier();
            changeLog.Analyze(File.ReadAllLines(filePath));
            changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_ADDED, publishModel.changeLogAdded);
            changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_CHANGED, publishModel.changeLogChanged);
            changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_REMOVED, publishModel.changeLogRemoved);
            changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_FIXED, publishModel.changeLogFixed);
            changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_OTHER, publishModel.changeLogOther);
            changeLog.EndEdit();
            FileHelper.SaveFileText(changeLog.ToString(), filePath);

            publishModel.currentVersion = ReadCurrentVersion(configModel);

        }

        string ReadCurrentVersion(GitRepoConfigModel configModel) {
            string dir = Application.dataPath;
            var filePath = Path.Combine(dir, configModel.versionFilePath);
            if (!File.Exists(filePath)) {
                return "unknown";
            }
            string version = FileHelper.LoadTextFromFile(filePath);
            return version;
        }

        // ==== 配置文件相关 ====
        void CreateDataFile() {
            GitRepoConfigModel model = new GitRepoConfigModel();
            SaveDataFile(model);
        }

        void SaveDataFile(GitRepoConfigModel model) {
            string jsonStr = JsonConvert.SerializeObject(model);
            FileHelper.SaveFileText(jsonStr, GetFilePath());
            configModel = model;
            EditorUtility.DisplayDialog("保存结果", "成功", "确认");
            AssetDatabase.Refresh();
        }

        string GetFilePath() {
            return Path.Combine(Application.dataPath, "GitPublishConfig.txt");
        }

        GitRepoConfigModel ReadDataFile() {
            if (!File.Exists(GetFilePath())) {
                return null;
            }
            string jsonStr = FileHelper.LoadTextFromFile(GetFilePath());
            GitRepoConfigModel model = JsonConvert.DeserializeObject<GitRepoConfigModel>(jsonStr);
            return model;
        }

    }

}
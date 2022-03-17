using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

namespace JackFrame.EditorTool {

    public class GitRepoPublishEditorWindow : EditorWindow {

        static GitRepoConfigModel data;
        int toolbarIndex = 0;

        [MenuItem(MENU_CONTEXT_NAME.L1_TOOL + "/GitPublish")]
        public static void Open() {
            GitRepoPublishEditorWindow window = EditorWindow.GetWindow<GitRepoPublishEditorWindow>();
            window.titleContent.text = "Git发布工具";
            window.Show();
        }

        void OnEnable() {
            if (data == null) {
                data = ReadDataFile();
            }
        }

        void OnGUI() {

            if (data == null) {
                if (GUILayout.Button("创建配置文件")) {
                    CreateDataFile();
                }
                if (GUILayout.Button("重试读取配置文件")) {
                    data = ReadDataFile();
                }
                return;
            }

            toolbarIndex = GUILayout.Toolbar(toolbarIndex, new string[] { "基本配置", "准备发布" });

            if (toolbarIndex == 0) {

                GUILayout.Label("CHANGELOG.md 所在路径");
                data.changeLogFilePath = GUILayout.TextField(data.changeLogFilePath);

                GUILayout.Space(10);
                GUILayout.Label("VERSION 所在路径");
                data.versionFilePath = GUILayout.TextField(data.versionFilePath);

                GUILayout.Space(10);
                GUILayout.Label("package.json 所在路径");
                data.packageJsonFilePath = GUILayout.TextField(data.packageJsonFilePath);

                GUILayout.Space(10);
                if (GUILayout.Button("保存")) {
                    SaveDataFile(data);
                }

            } else if (toolbarIndex == 1) {

                GUILayout.Label("待发布版本号(例1.1.0, 前后不加符号)");
                data.semanticVersion = GUILayout.TextField(data.semanticVersion);

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Added");
                data.changeLogAdded = GUILayout.TextArea(data.changeLogAdded, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Changed");
                data.changeLogChanged = GUILayout.TextArea(data.changeLogChanged, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Removed");
                data.changeLogRemoved = GUILayout.TextArea(data.changeLogRemoved, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Fixed");
                data.changeLogFixed = GUILayout.TextArea(data.changeLogFixed, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Other");
                data.changeLogOther = GUILayout.TextArea(data.changeLogOther, GUILayout.MinHeight(50));

                if (GUILayout.Button("保存发布")) {
                    string title = "发布确认";
                    string content = $"以下文件将会被修改:\r\n"
                                    + $"  {data.changeLogFilePath}\r\n"
                                    + $"  {data.versionFilePath}\r\n"
                                    + $"  {data.packageJsonFilePath}\r\n"
                                    + $"是否确认保存?";
                    string ok = "确认";
                    string cancel = "取消";
                    if (EditorUtility.DisplayDialog(title, content, ok, cancel)) {
                        SaveChange(data);
                    }
                } 

            }

        }

        // ==== 保存发布信息到文件 ====
        void SaveChange(GitRepoConfigModel data) {

            const string NOTICE = "请放置在Assets目录下, 并且路径字符串不需要添加Assets前缀";

            string dir = Application.dataPath;
            string filePath;
            // SAVE VERSION
            //      COVER VERSION
            filePath = Path.Combine(dir, data.versionFilePath);
            if (!File.Exists(filePath)) {
                EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
                return;
            }
            FileHelper.SaveFileText(data.semanticVersion, filePath);

            // class PackageInfo
            // SAVE PACKAGEJSON:
            //      VERSION
            filePath = Path.Combine(dir, data.packageJsonFilePath);
            if (!File.Exists(filePath)) {
                EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
                return;
            }
            string jsonStr = FileHelper.LoadTextFromFile(filePath);
            PackageJsonObj json = JsonConvert.DeserializeObject<PackageJsonObj>(jsonStr);
            json.version = data.semanticVersion;
            jsonStr = JsonConvert.SerializeObject(json, Formatting.Indented);
            FileHelper.SaveFileText(jsonStr, filePath);

            // SAVE CHANGELOG:
            //      VERSION
            //      ADDED/CHANGED/REMOVED/FIXED/OTHER
            filePath = Path.Combine(dir, data.changeLogFilePath);
            if (!File.Exists(filePath)) {
                EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
                return;
            }
            ChangeLogModifier changeLog = new ChangeLogModifier();
            changeLog.Analyze(File.ReadAllLines(filePath));
            changeLog.AddInfo(data.semanticVersion, ChangeLogElementTag.TAG_ADDED, data.changeLogAdded);
            changeLog.AddInfo(data.semanticVersion, ChangeLogElementTag.TAG_CHANGED, data.changeLogChanged);
            changeLog.AddInfo(data.semanticVersion, ChangeLogElementTag.TAG_REMOVED, data.changeLogRemoved);
            changeLog.AddInfo(data.semanticVersion, ChangeLogElementTag.TAG_FIXED, data.changeLogFixed);
            changeLog.AddInfo(data.semanticVersion, ChangeLogElementTag.TAG_OTHER, data.changeLogOther);
            changeLog.EndEdit();
            FileHelper.SaveFileText(changeLog.ToString(), filePath);

        }

        // ==== 配置文件相关 ====
        void CreateDataFile() {
            GitRepoConfigModel model = new GitRepoConfigModel();
            SaveDataFile(model);
        }

        void SaveDataFile(GitRepoConfigModel model) {
            string jsonStr = JsonConvert.SerializeObject(model);
            FileHelper.SaveFileText(jsonStr, GetFilePath());
            data = model;
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
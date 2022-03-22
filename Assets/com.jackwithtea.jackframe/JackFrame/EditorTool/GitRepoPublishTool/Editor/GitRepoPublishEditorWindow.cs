using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

namespace JackFrame.EditorTool {

    public class GitRepoPublishEditorWindow : EditorWindow {

        GitRepoConfigModel gitRepoConfigModel;
        UnityPackageConfigModel unityPackageConfigModel;
        [SerializeField] PublishSo publishSo;
        int toolbarIndex = 0;

        [MenuItem(MENU_CONTEXT_NAME.L1_TOOL + "/GitPublish")]
        public static void Open() {
            GitRepoPublishEditorWindow window = EditorWindow.GetWindow<GitRepoPublishEditorWindow>();
            window.titleContent.text = "Git发布工具";
            window.Show();
        }

        void OnEnable() {
            if (gitRepoConfigModel == null) {
                gitRepoConfigModel = ReadGitRepoConfigFile();
            }
            if (unityPackageConfigModel == null) {
                unityPackageConfigModel = ReadUnityPackageConfigFile();
            }
            if (publishSo == null) {
                publishSo = ScriptableObject.CreateInstance<PublishSo>();
            }
        }

        void OnGUI() {

            toolbarIndex = GUILayout.Toolbar(toolbarIndex, new string[] { "基本配置", "准备发布", "打包 UnityPackage" });

            if (toolbarIndex == 0) {

                if (gitRepoConfigModel == null) {
                    if (GUILayout.Button("创建 GitRepo 配置文件")) {
                        CreateGitRepoConfigFile();
                    }
                    if (GUILayout.Button("重试 GitRepo 读取配置文件")) {
                        gitRepoConfigModel = ReadGitRepoConfigFile();
                    }
                    return;
                }

                GUILayout.Label("CHANGELOG.md 所在路径");
                gitRepoConfigModel.changeLogFilePath = GUILayout.TextField(gitRepoConfigModel.changeLogFilePath);

                GUILayout.Space(10);
                GUILayout.Label("VERSION 所在路径");
                gitRepoConfigModel.versionFilePath = GUILayout.TextField(gitRepoConfigModel.versionFilePath);

                GUILayout.Space(10);
                GUILayout.Label("package.json 所在路径");
                gitRepoConfigModel.packageJsonFilePath = GUILayout.TextField(gitRepoConfigModel.packageJsonFilePath);

                GUILayout.Space(10);
                if (GUILayout.Button("保存")) {
                    SaveGitRepoConfigFile(gitRepoConfigModel);
                }

            } else if (toolbarIndex == 1) {

                if (publishSo.currentVersion == null) {
                    publishSo.currentVersion = ReadCurrentVersion(gitRepoConfigModel);
                }

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"当前版本号: {publishSo.currentVersion}");
                if (GUILayout.Button("重新获取")) {
                    publishSo.currentVersion = ReadCurrentVersion(gitRepoConfigModel);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.Label("待发布版本号(例1.1.0, 前后不加符号)");
                publishSo.semanticVersion = GUILayout.TextField(publishSo.semanticVersion);

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Added");
                publishSo.changeLogAdded = GUILayout.TextArea(publishSo.changeLogAdded, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Changed");
                publishSo.changeLogChanged = GUILayout.TextArea(publishSo.changeLogChanged, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Removed");
                publishSo.changeLogRemoved = GUILayout.TextArea(publishSo.changeLogRemoved, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Fixed");
                publishSo.changeLogFixed = GUILayout.TextArea(publishSo.changeLogFixed, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                GUILayout.Label("ChangeLog Other");
                publishSo.changeLogOther = GUILayout.TextArea(publishSo.changeLogOther, GUILayout.MinHeight(50));

                if (GUILayout.Button("保存发布")) {
                    string title = "发布确认";
                    string content = $"以下文件将会被修改:\r\n"
                                    + $"  {gitRepoConfigModel.changeLogFilePath}\r\n"
                                    + $"  {gitRepoConfigModel.versionFilePath}\r\n"
                                    + $"  {gitRepoConfigModel.packageJsonFilePath}\r\n"
                                    + $"是否确认保存?";
                    string ok = "确认";
                    string cancel = "取消";
                    if (EditorUtility.DisplayDialog(title, content, ok, cancel)) {
                        SaveChange(gitRepoConfigModel, publishSo);
                    }
                }

            } else if (toolbarIndex == 2) {

                if (unityPackageConfigModel == null) {
                    if (GUILayout.Button("创建 UnityPackage 配置文件")) {
                        unityPackageConfigModel = CreateUnityPackageConfigFile();
                    }
                    return;
                }

                GUILayout.Label($"当前版本号: {publishSo.currentVersion}");

                GUILayout.Space(10);
                GUILayout.Label("源目录");
                unityPackageConfigModel.packageSourceDir = GUILayout.TextField(unityPackageConfigModel.packageSourceDir);

                GUILayout.Space(10);
                GUILayout.Label("导出目录(注: 该地址基于Environment.CurrentDirectory)");
                unityPackageConfigModel.packageOutputDir = GUILayout.TextField(unityPackageConfigModel.packageOutputDir);

                GUILayout.Space(10);
                GUILayout.Label("文件名");
                unityPackageConfigModel.packageName = GUILayout.TextField(unityPackageConfigModel.packageName);

                GUILayout.Space(10);
                unityPackageConfigModel.isAutoVersion = GUILayout.Toggle(unityPackageConfigModel.isAutoVersion, "是否自动附加版本号");

                GUILayout.Space(10);
                if (GUILayout.Button("保存配置")) {
                    SaveUnityPackageConfigFile(unityPackageConfigModel);
                }

                if (GUILayout.Button("打包")) {
                    FileHelper.CreateDirIfNorExist(unityPackageConfigModel.packageOutputDir);
                    string inputDir = Path.Combine("Assets", unityPackageConfigModel.packageSourceDir);
                    UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(inputDir);
                    var list = new List<string>();
                    for (int i = 0; i < objs.Length; i += 1) {
                        var obj = objs[i];
                        var path = AssetDatabase.GetAssetPath(obj);
                        list.Add(path);
                    }
                    string outputFile = Path.Combine(unityPackageConfigModel.packageOutputDir, unityPackageConfigModel.packageName + ReadCurrentVersion(gitRepoConfigModel) + ".unitypackage");
                    AssetDatabase.ExportPackage(list.ToArray(),  outputFile, ExportPackageOptions.Recurse);
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
            filePath = Path.Combine(dir, config.versionFilePath.TrimStart('/'));
            if (!File.Exists(filePath)) {
                EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
                return;
            }
            FileHelper.SaveFileText(publishModel.semanticVersion, filePath);

            // class PackageInfo
            // SAVE PACKAGEJSON:
            //      VERSION
            filePath = Path.Combine(dir, config.packageJsonFilePath.TrimStart('/'));
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
            filePath = Path.Combine(dir, config.changeLogFilePath.TrimStart('/'));
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

            publishModel.currentVersion = ReadCurrentVersion(gitRepoConfigModel);

        }

        string ReadCurrentVersion(GitRepoConfigModel configModel) {
            string dir = Application.dataPath;
            var filePath = Path.Combine(dir, configModel.versionFilePath.TrimStart('/'));
            if (!File.Exists(filePath)) {
                return "unknown";
            }
            string version = FileHelper.LoadTextFromFile(filePath);
            return version;
        }

        // ==== 配置文件相关 ====
        // -- GIT REPO
        void CreateGitRepoConfigFile() {
            GitRepoConfigModel model = new GitRepoConfigModel();
            SaveGitRepoConfigFile(model);
        }

        void SaveGitRepoConfigFile(GitRepoConfigModel model) {
            string jsonStr = JsonConvert.SerializeObject(model);
            FileHelper.SaveFileText(jsonStr, GetGitRepoConfigFilePath());
            gitRepoConfigModel = model;
            EditorUtility.DisplayDialog("保存结果", "成功", "确认");
            AssetDatabase.Refresh();
        }

        string GetGitRepoConfigFilePath() {
            return Path.Combine(Application.dataPath, "GitPublishConfig.txt");
        }

        GitRepoConfigModel ReadGitRepoConfigFile() {
            if (!File.Exists(GetGitRepoConfigFilePath())) {
                return null;
            }
            string jsonStr = FileHelper.LoadTextFromFile(GetGitRepoConfigFilePath());
            GitRepoConfigModel model = JsonConvert.DeserializeObject<GitRepoConfigModel>(jsonStr);
            return model;
        }

        // -- UNITY PACKAGE
        string GetUnityPackageConfigFile() {
            return Path.Combine(Application.dataPath, "UnityPackageConfig.txt");
        }

        UnityPackageConfigModel CreateUnityPackageConfigFile() {
            UnityPackageConfigModel model = new UnityPackageConfigModel();
            SaveUnityPackageConfigFile(model);
            return model;
        }

        UnityPackageConfigModel ReadUnityPackageConfigFile() {
            if (!File.Exists(GetUnityPackageConfigFile())) {
                return null;
            }
            string jsonStr = FileHelper.LoadTextFromFile(GetUnityPackageConfigFile());
            UnityPackageConfigModel model = JsonConvert.DeserializeObject<UnityPackageConfigModel>(jsonStr);
            return model;
        }

        void SaveUnityPackageConfigFile(UnityPackageConfigModel model) {
            string jsonStr = JsonConvert.SerializeObject(model);
            FileHelper.SaveFileText(jsonStr, GetUnityPackageConfigFile());
            unityPackageConfigModel = model;
            EditorUtility.DisplayDialog("保存结果", "成功", "确认");
            AssetDatabase.Refresh();
        }

    }

}
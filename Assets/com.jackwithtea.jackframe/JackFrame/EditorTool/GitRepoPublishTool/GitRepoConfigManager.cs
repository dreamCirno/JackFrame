using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace JackFrame.EditorTool {

    public class GitRepoConfigManager : MonoBehaviour {

        [Header("基本信息")]
        public string changeLogFilePath;
        public string versionFilePath;
        public string packageJsonFilePath;

        [Header("提交信息")]
        [TextArea(1,1)]
        [Tooltip("填写格式: 1.0.1")]
        public string semanticVersion;

        [TextArea(3, 6)]
        [Tooltip("<Added>/<Removed>/<Changed>/<Fixed>")]
        public string changeLogInfo;

        public void SaveChange() {

            string dir = Application.dataPath;
            string filePath;
            // SAVE VERSION
            //      COVER VERSION
            filePath = Path.Combine(dir, versionFilePath);
            FileHelper.SaveFileText(filePath, semanticVersion);

            // class PackageInfo
            // SAVE PACKAGEJSON:
            //      VERSION
            filePath = Path.Combine(dir, packageJsonFilePath);
            string jsonStr = FileHelper.LoadTextFromFile(filePath);
            PackageJsonObj json = JsonConvert.DeserializeObject<PackageJsonObj>(jsonStr);
            json.version = semanticVersion;
            jsonStr = JsonConvert.SerializeObject(json, Formatting.Indented);
            FileHelper.SaveFileText(jsonStr, filePath);

            // SAVE CHANGELOG:
            //      VERSION
            //      ADDED/REMOVED ...
            filePath = Path.Combine(dir, changeLogFilePath);
            ChangeLogModifier changeLog = new ChangeLogModifier();
            changeLog.Analyze(File.ReadAllLines(filePath));
            changeLog.InsertVersionBeforeLastVersion(semanticVersion, changeLogInfo);
            FileHelper.SaveFileText(changeLog.ToString(), filePath);

        }

        [ContextMenu("LogInfo")]
        public void Log() {
            var dir = Application.dataPath;
            var str = FileHelper.LoadTextFromFile(Path.Combine(dir, changeLogFilePath));
            Debug.Log("CHANGELOG:\r\n" + str);

            str = FileHelper.LoadTextFromFile(Path.Combine(dir, versionFilePath));
            Debug.Log("VERSION:\r\n" + str);

            str = FileHelper.LoadTextFromFile(Path.Combine(dir, packageJsonFilePath));
            Debug.Log("PACKAGE JSON:\r\n" + str);

        }

    }

}
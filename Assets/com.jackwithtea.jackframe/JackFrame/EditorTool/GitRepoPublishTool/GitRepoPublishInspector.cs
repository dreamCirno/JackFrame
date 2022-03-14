using UnityEngine;
using UnityEditor;

namespace JackFrame.EditorTool {

    [CustomEditor(typeof(GitRepoConfigManager))]
    public class GitRepoPublishInspector : Editor {

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var obj = (GitRepoConfigManager)target;
            if (GUILayout.Button("保存发布")) {
                string title = "发布确认";
                string content = $"以下文件将会被修改:\r\n"
                                + $"  {obj.changeLogFilePath}\r\n"
                                + $"  {obj.versionFilePath}\r\n"
                                + $"  {obj.packageJsonFilePath}\r\n"
                                + $"是否确认保存?";
                string ok = "确认";
                string cancel = "取消";
                if (EditorUtility.DisplayDialog(title, content, ok, cancel)) {
                    obj.SaveChange();
                }
            }
        }

    }

}
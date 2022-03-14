#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.CodeEditor;
using VSCodeEditor;
using JackFrame; 

namespace JackFrame.EditorTool {

    public class RegenerateCSProjTool {

        [MenuItem(MENU_CONTEXT_NAME.L1_TOOL + "/RegenerateCSProj")]
        public static void CleanCSProj() {

            List<string> files = FileHelper.FindAllFileWithExt(Environment.CurrentDirectory, "*.csproj");
            foreach (var file in files) {
                File.Delete(file);
            }
            Debug.Log("消除 CSProj 成功: " + files.Count.ToString());

            IExternalCodeEditor codeEditor = CodeEditor.CurrentEditor;
            VSCodeScriptEditor vSCodeScriptEditor = codeEditor as VSCodeScriptEditor;
            vSCodeScriptEditor.SyncAll();

            FieldInfo info = vSCodeScriptEditor.GetType().GetField("m_ProjectGeneration", BindingFlags.NonPublic | BindingFlags.Instance);
            IGenerator generator = info.GetValue(vSCodeScriptEditor) as IGenerator;
            generator.Sync();

            Debug.Log("重新生成了 CSProj");
        }

    }
}
#endif

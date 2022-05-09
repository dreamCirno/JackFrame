using UnityEditor;
using UnityEditor.Build;

namespace JackFrame.EditorTool {

    public static class ScriptDefineUtil {

        public static void AddGlobalDefineIfNotExist(string define, NamedBuildTarget buildTarget) {

            PlayerSettings.GetScriptingDefineSymbols(buildTarget, out string[] defindes);
            for (int i = 0; i < defindes.Length; i += 1) {
                if (defindes[i] == define) {
                    return;
                }
            }

            string[] newDefine = new string[defindes.Length + 1];
            defindes.CopyTo(newDefine, 0);
            newDefine[defindes.Length] = define;
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, newDefine);

        }
    }
}
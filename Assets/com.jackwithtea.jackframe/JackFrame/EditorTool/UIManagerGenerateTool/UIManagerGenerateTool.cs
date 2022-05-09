using System;
using UnityEngine;
using UnityEngine.UI;

namespace JackFrame.EditorTool {

    public static class UIManagerGenerateTool {

        [UnityEditor.MenuItem("GameObject/" + MENU_CONTEXT_NAME.L1 + "/UI/Generate UIManager")]
        public static void Generate() {
            var prefab = Resources.Load("UI_CANVAS") as GameObject;
            var selected = UnityEditor.Selection.activeGameObject;
            prefab = GameObject.Instantiate(prefab, selected?.transform);
            prefab.name = prefab.name.Replace("(Clone)", "");
        }
    }

}
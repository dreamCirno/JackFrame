using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace JackFrame.EditorTool {

    public static class UIManagerGenerateTool {

        [UnityEditor.MenuItem("GameObject/" + nameof(JackFrame) + "/UI/Generate UIManager")]
        public static void Generate() {
            var selected = Selection.activeGameObject;
            var prefab = Resources.Load("UI_CANVAS") as GameObject;
            prefab = PrefabUtility.InstantiatePrefab(prefab, selected?.transform) as GameObject;
            prefab.name = prefab.name.Replace("(Clone)", "");
        }
    }

}
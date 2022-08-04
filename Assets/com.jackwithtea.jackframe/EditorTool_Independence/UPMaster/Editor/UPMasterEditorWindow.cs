using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using Newtonsoft.Json;

namespace JackFrame.UPMaster {

    public class UPMasterEditorWindow : EditorWindow {

        string CONFIG_DIR => Application.dataPath + "/Config";
        string DEP_FILE_PATH => CONFIG_DIR + "/UPMaster.json";

        Dictionary<string, UPMasterDependancyModel> all;

        List<UPMasterDependancyModel> inputList;
        Queue<UPMasterDependancyModel> toRemoveQueue;

        UPMManifestModifier modifier;

        [MenuItem(nameof(JackFrame) + "/UPMaster/OpenWindow")]
        public static void OpenWindow() {
            var window = EditorWindow.GetWindow<UPMasterEditorWindow>();
            window.Show();
        }

        void OnEnable() {
            Initialize();
        }

        // ==== GUI ====
        void OnGUI() {

            GUILayout.Space(10);

            DequeueRemoveDependencies();

            int index = 0;

            foreach (var model in all.Values) {
                GUI_DrawInputLine(model, index);
                index += 1;
            }

            inputList.ForEach(model => {
                GUI_DrawInputLine(model, index);
                index += 1;
            });

            if (GUILayout.Button("添加依赖")) {
                inputList.Add(new UPMasterDependancyModel());
            }

            if (GUILayout.Button("保存")) {
                Save();
                ModifyManifest();
            }

        }

        void GUI_DrawInputLine(UPMasterDependancyModel model, int index) {

            GUILayout.Box("Index: " + index.ToString());

            const float LABEL_WIDTH = 80;

            GUILayout.BeginHorizontal();
            GUILayout.Label("包名:", GUILayout.Width(LABEL_WIDTH));
            model.name = GUILayout.TextField(model.name);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Git URL:", GUILayout.Width(LABEL_WIDTH));
            model.gitUrl = GUILayout.TextField(model.gitUrl);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Git Target Dir:", GUILayout.Width(LABEL_WIDTH));
            model.targetDir = GUILayout.TextField(model.targetDir);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Git Branch:", GUILayout.Width(LABEL_WIDTH));
            model.branchOrTag = GUILayout.TextField(model.branchOrTag);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("移除依赖")) {
                toRemoveQueue.Enqueue(model);
            }

            GUILayout.Space(20);

        }

        // ==== LOGIC ====
        void Initialize() {

            inputList = new List<UPMasterDependancyModel>();
            toRemoveQueue = new Queue<UPMasterDependancyModel>();

            CreateOrLoadDefaultConfig();

            modifier = new UPMManifestModifier();
            modifier.Initialize();

        }

        void CreateOrLoadDefaultConfig() {

            all = new Dictionary<string, UPMasterDependancyModel>();
            if (!Directory.Exists(CONFIG_DIR)) {
                Directory.CreateDirectory(CONFIG_DIR);
            }

            if (File.Exists(DEP_FILE_PATH)) {
                string dataStr = File.ReadAllText(DEP_FILE_PATH);
                UPMasterDependancyModel[] arr = JsonConvert.DeserializeObject<UPMasterDependancyModel[]>(dataStr);
                if (arr == null) {
                    return;
                }
                foreach (var model in arr) {
                    all.Add(model.name, model);
                }
            } else {
                UPMasterDependancyModel[] arr = new UPMasterDependancyModel[1] {
                    new UPMasterDependancyModel()
                };
                string json = JsonConvert.SerializeObject(arr);
                File.WriteAllText(DEP_FILE_PATH, json);
            }

        }

        void Save() {

            // 查重名
            foreach (var value in all.Values) {
                int tarIndex = inputList.FindIndex(tar => tar.name == value.name);
                if (tarIndex != -1) {
                    Debug.LogError("有重名包: " + inputList[tarIndex].name);
                    return;
                }
            }

            // 添加进 Dic
            // Clear List
            inputList.ForEach(model => all.Add(model.name, model));
            inputList.Clear();

            // 写入文件
            UPMasterDependancyModel[] dataArr = all.Values.ToArray();
            string dataToWrite = JsonConvert.SerializeObject(dataArr);
            File.WriteAllText(DEP_FILE_PATH, dataToWrite);

        }

        void DequeueRemoveDependencies() {
            while (toRemoveQueue.TryDequeue(out var model)) {
                all.Remove(model.name);
                inputList.Remove(model);
                modifier.Remove(model.name);
            }
        }

        void ModifyManifest() {
            foreach (var model in all.Values) {
                modifier.AddOrReplace(model.name, model.GetFullURL());
            }

            var json = modifier.Generate();
            Debug.Log("GEN:" + json);
            string path = Path.Combine(Environment.CurrentDirectory, "Packages", "manifest.json");
            File.WriteAllText(path, json);
        }

    }

}
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JackFrame.UPMaster {

    public class UPMManifestModifier {

        class DepModel {
            public string name;
            public string version;

            public DepModel(string name, string version) {
                this.name = name;
                this.version = version;
            }
        }

        string json;

        List<DepModel> all;

        public UPMManifestModifier() {
            this.all = new List<DepModel>();
        }

        public void Initialize() {

            string path = Path.Combine(Environment.CurrentDirectory, "Packages", "manifest.json");
            this.json = File.ReadAllText(path);

            // TRIM
            json = json.Replace("\r\n", "");
            json = json.Replace(" ", "");

            // CACHE ALL
            var arr = json.Split("{");
            arr = arr[2].Split("}");
            arr = arr[0].Split(",");
            foreach (var str in arr) {
                var kv = str.Split(":");
                string key = kv[0];
                string value = kv[1];
                var model = new DepModel(key.Replace("\"", ""), value.Replace("\"", ""));
                all.Add(model);
            }

        }

        public void Add(string key, string value) {
            int existIndex = all.FindIndex(value => value.name == key);
            if (existIndex != -1) {
                return;
            } else {
                all.Add(new DepModel(key, value));
            }
        }

        public void AddOrReplace(string key, string value) {
            int existIndex = all.FindIndex(value => value.name == key);
            if (existIndex != -1) {
                var model = all[existIndex];
                model.version = value;
            } else {
                all.Add(new DepModel(key, value));
            }
        }

        public void Replace(string key, string value) {
            int existIndex = all.FindIndex(value => value.name == key);
            if (existIndex != -1) {
                var model = all[existIndex];
                model.version = value;
            }
        }

        public void Remove(string key) {
            int existIndex = all.FindIndex(value => value.name == key);
            if (existIndex != -1) {
                all.RemoveAt(existIndex);
            }
        }

        public string Generate() {
            string data = "";
            int index = 0;
            all.ForEach(value => {
                string per;
                Debug.Log("PER: " + value.version);
                if (index == all.Count - 1) {
                    per = "\"" + value.name + "\":\"" + @value.version + "\"\r\n";
                } else {
                    per = "\"" + value.name + "\":\"" + @value.version + "\",\r\n";
                }
                data += per;
                index += 1;
            });
            string str = "{\r\n\"dependencies\": {\r\n" + data + "}\r\n}";
            return str;
        }

    }
}
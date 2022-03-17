using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace JackFrame.EditorTool {

    public class GitRepoConfigModel {

        public string changeLogFilePath = "/CHANGELOG.md";
        public string versionFilePath = "/VERSION";
        public string packageJsonFilePath = "/package.json";

        public string semanticVersion = "1.0.0";

        public string changeLogAdded = "";
        public string changeLogChanged = "";
        public string changeLogRemoved = "";
        public string changeLogFixed = "";
        public string changeLogOther = "";

    }

}
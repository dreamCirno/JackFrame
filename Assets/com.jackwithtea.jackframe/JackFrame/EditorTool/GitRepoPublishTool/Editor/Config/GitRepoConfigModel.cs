using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace JackFrame.EditorTool {

    public class GitRepoConfigModel {

        public string changeLogFilePath = "/CHANGELOG.md";
        public string versionFilePath = "/VERSION";
        public string packageJsonFilePath = "/package.json";

    }

}
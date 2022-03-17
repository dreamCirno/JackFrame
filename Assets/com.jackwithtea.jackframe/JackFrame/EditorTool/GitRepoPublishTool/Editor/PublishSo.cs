using UnityEngine;

namespace JackFrame.EditorTool {

    public class PublishSo : ScriptableObject {

        public string currentVersion;
        public string semanticVersion = "1.0.0";

        public string changeLogAdded = "";
        public string changeLogChanged = "";
        public string changeLogRemoved = "";
        public string changeLogFixed = "";
        public string changeLogOther = "";

    }

}
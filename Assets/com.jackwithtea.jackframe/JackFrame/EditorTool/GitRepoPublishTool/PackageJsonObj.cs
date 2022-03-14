using System.Collections.Generic;
using UnityEngine;

namespace JackFrame.EditorTool {

    [HelpURL("https://docs.unity3d.com/Manual/upm-manifestPkg.html")]
    public class PackageJsonObj {

        // REQUIRED PROPERTIES
        public string name = "";
        public string version = "";

        // RECOMMANDED PROPERTIES
        public string description = "";
        public string displayName = "";
        public string unity = "";

        // OPTIONAL PROPERTIES
        public AutohrObj author = new AutohrObj();
        public string changelogUrl = "";
        public Dictionary<string, string> dependencies;
        public string documentationUrl = "";
        public bool hideInEditor = false;
        public string[] keywords = new string[0];
        public string license = "";
        public string licensesUrl = "";
        public SampleObj[] samples = new SampleObj[0];
        // public string type = "";
        public string unityRelease = "";

        public class AutohrObj {
            public string name = "";
            public string email = "";
            public string url = "";
        }

        public class SampleObj {
            public string displayName = "";
            public string description = "";
            public string path = "";
        }

    }

}
using System.Collections.Generic;
using UnityEngine;

namespace JackFrame.EditorTool {

    [HelpURL("https://docs.unity3d.com/Manual/upm-manifestPkg.html")]
    public class PackageJsonObj {

        // REQUIRED PROPERTIES
        public string name = "anonymous";
        public string version = "0.0.0";

        // RECOMMANDED PROPERTIES
        public string description = "anonymous";
        public string displayName = "anonymous";
        public string unity = "";

        // OPTIONAL PROPERTIES
        public AutohrObj author = new AutohrObj();
        public string changelogUrl = "";
        public Dictionary<string, string> dependencies = new Dictionary<string, string>();
        public string documentationUrl = "";
        public bool hideInEditor = false;
        public string[] keywords = new string[0];
        public string license = "";
        public string licensesUrl = "";
        public SampleObj[] samples = new SampleObj[0];
        // public string type = "";
        public string unityRelease = "";

        public class AutohrObj {
            public string name = "anonymous";
            public string email = "anonymous@anonymous.com";
            public string url = "http://anonymous.com";
        }

        public class SampleObj {
            public string displayName = "anonymous";
            public string description = "anonymous";
            public string path = "/";
        }

    }

}
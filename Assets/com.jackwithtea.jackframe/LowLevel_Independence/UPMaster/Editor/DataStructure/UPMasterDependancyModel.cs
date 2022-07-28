using System;

namespace JackFrame.UPMaster {

    [Serializable]
    public class UPMasterDependancyModel {

        public string name;
        public string gitUrl;
        public string targetDir;
        public string branchOrTag;

        public UPMasterDependancyModel() {
            this.name = "";
            this.gitUrl = "";
            this.targetDir = "";
            this.branchOrTag = "";
        }

        public string GetFullURL() {
            var realUrl = gitUrl.Replace(".com:", ".com/");
            return realUrl + "?path=" + targetDir + "#" + branchOrTag;
        }

        public bool Check() {

            if (string.IsNullOrEmpty(name)) {
                return false;
            }

            if (string.IsNullOrEmpty(gitUrl)) {
                return false;
            }

            if (string.IsNullOrEmpty(targetDir)) {
                return false;
            }

            if (string.IsNullOrEmpty(branchOrTag)) {
                return false;
            }

            if (!gitUrl.StartsWith("https://") && !gitUrl.StartsWith("ssh://")) {
                return false;
            }

            return true;

        }

    }

}
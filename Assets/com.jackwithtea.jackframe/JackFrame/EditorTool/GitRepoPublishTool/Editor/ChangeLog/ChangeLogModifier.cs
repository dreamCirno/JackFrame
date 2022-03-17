using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

namespace JackFrame.EditorTool {

    public class ChangeLogModifier {

        const string VERSION_LINE_STRATS_WITH = "## ";
        const string VERSION_ELEMENT_STRATS_WITH = "### ";

        List<string> titleLineList;
        List<VersionContainer> versionList;
        VersionContainer editingVersion;
        bool isUseOldVersion = false;

        public ChangeLogModifier() {
            this.titleLineList = new List<string>();
            this.versionList = new List<VersionContainer>();
        }

        public void Analyze(string[] changeLogTxtLines) {

            VersionContainer curContainer = null;
            VersionElement curElement = null;

            for (int i = 0; i < changeLogTxtLines.Length; i += 1) {
                string line = changeLogTxtLines[i];
                if (line.StartsWith(VERSION_LINE_STRATS_WITH)) {
                    string semanticVersion = line.GetMatchesLettersBetweenTwoChar("[", "]")[0].Value;
                    if (curContainer == null || curContainer.semanticVersion != semanticVersion) {
                        curContainer = GetOrAddVersion(semanticVersion, line);
                    }
                } else if (line.StartsWith(VERSION_ELEMENT_STRATS_WITH)) {
                    if (curContainer == null) {
                        continue;
                    }
                    string tag = line.Split(VERSION_ELEMENT_STRATS_WITH)[1];
                    curElement = curContainer.GetOrAddElementTag(tag);
                } else {
                    if (curElement == null) {
                        titleLineList.Add(line);
                        continue;
                    }
                    curContainer.AddElement(curElement.tag, line);
                }
            }

        }

        public void AddInfo(string semanticVersion, string tag, string content) {

            if (editingVersion == null) {
                editingVersion = versionList.Find(value => value.semanticVersion == semanticVersion);
                if (editingVersion != null) {
                    isUseOldVersion = true;
                } else {
                    isUseOldVersion = false;
                    editingVersion = new VersionContainer(semanticVersion, ToFullVersionFormat(semanticVersion), true);
                }
            }

            VersionElement curEle = null;
            curEle = editingVersion.GetOrAddElementTag(tag);
            string[] lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i += 1) {
                string line = lines[i];
                if (curEle != null) {
                    editingVersion.AddElement(curEle.tag, line);
                }
            }
        }

        public void EndEdit() {
            if (isUseOldVersion) {
                return;
            }
            int lastVersionIndex = FindLastVersionIndex();
            if (lastVersionIndex == -1) {
                versionList.Add(editingVersion);
            } else {
                versionList.Insert(lastVersionIndex, editingVersion);
            }
        }

        VersionContainer GetOrAddVersion(string semanticVersion, string fullVersionLine) {
            int index = versionList.FindIndex(value => value.semanticVersion == semanticVersion);
            if (index == -1) {
                var versionContainer = new VersionContainer(semanticVersion, fullVersionLine, false);
                versionList.Add(versionContainer);
                return versionContainer;
            } else {
                return versionList[index];
            }
        }

        int FindLastVersionIndex() {
            int index = versionList.FindIndex(value => value.semanticVersion.Contains("."));
            return index;
        }

        string ToFullVersionFormat(string semanticVersion) {
            return $"## [{semanticVersion}] - " + ToDateFormat();
        }

        string ToDateFormat() {
            return DateTime.Now.ToYYYYMMDD();
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            titleLineList.ForEach(value => {
                sb.AppendLine(value);
            });
            versionList.ForEach(value => {
                sb.AppendLine(value.fullVersion);
                value.elementList.ForEach(ele => {
                    if (!ele.HasContent()) {
                        return;
                    }
                    string tag = $"### {ele.tag}";
                    sb.AppendLine(tag);
                    ele.ForEach(content => {
                        if (value.isNewVersion) {
                            sb.AppendLine("- " + content + "  ");
                        } else {
                            sb.AppendLine(content);
                        }
                    });
                });
                if (value.isNewVersion) {
                    sb.AppendLine();
                }
            });
            return sb.ToString();
        }

        class VersionContainer {

            public string semanticVersion;
            public string fullVersion;
            public List<VersionElement> elementList;
            public bool isNewVersion;

            public VersionContainer(string semanticVersion, string srcLine, bool isNewVersion) {
                this.semanticVersion = semanticVersion;
                this.fullVersion = srcLine;
                this.isNewVersion = isNewVersion;
                this.elementList = new List<VersionElement>();
            }

            public VersionElement GetOrAddElementTag(string tag) {
                VersionElement ele = elementList.Find(value => value.tag == tag);
                if (ele == null) {
                    ele = new VersionElement();
                    ele.tag = tag;
                    elementList.Add(ele);
                }
                return ele;
            }

            public VersionElement AddElement(string tag, string content) {
                VersionElement ele = GetOrAddElementTag(tag);
                ele.AddContent(content);
                return ele;
            }

        }

        class VersionElement {

            public string tag;
            List<string> contentList;

            public VersionElement() {
                this.contentList = new List<string>();
            }

            public void AddContent(string content) {
                this.contentList.Add(content);
            }

            public bool HasContent() {
                return contentList.Count > 0 && contentList[0].TrimEnd() != "";
            }

            public void ForEach(Action<string> action) {
                contentList.ForEach(action);
            }

        }

    }

}
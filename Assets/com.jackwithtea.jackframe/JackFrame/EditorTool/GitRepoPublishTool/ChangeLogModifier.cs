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
                    string versionTxt = line.GetMatchesLettersBetweenTwoChar("[", "]")[0].Value;
                    if (curContainer == null || curContainer.version != versionTxt) {
                        curContainer = new VersionContainer(line);
                        curContainer.version = versionTxt;
                        versionList.Add(curContainer);
                    }
                } else if (line.StartsWith(VERSION_ELEMENT_STRATS_WITH)) {
                    if (curContainer == null) {
                        continue;
                    }
                    string tag = line.Split(VERSION_ELEMENT_STRATS_WITH)[1];
                    curElement = curContainer.AddElement(tag, "");
                } else {
                    if (curElement == null) {
                        titleLineList.Add(line);
                        continue;
                    }
                    curContainer.AddElement(curElement.tag, line);
                }
            }

        }

        public void InsertVersionBeforeLastVersion(string version, string content) {
            int index = versionList.FindIndex(value => value.version.Contains("."));
            string versionSrc = ToVersionFormat(version);
            VersionContainer versionContainer = new VersionContainer(versionSrc);
            if (index != -1) {
                versionList.Insert(index, versionContainer);
            } else {
                versionList.Add(versionContainer);
            }

            VersionElement curEle = null;
            string[] lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i += 1) {
                string line = lines[i];
                if (VersionElement.StartsWithTag(line, out string tag)) {
                    // IS TAG
                    curEle = versionContainer.AddElement(tag, "");
                } else {
                    // IS CONTENT
                    if (curEle != null) {
                        versionContainer.AddElement(curEle.tag, line);
                    }
                }
            }
        }

        int IndexOfLastVersion(string tempTxt) {
            int target = -1;

            // DO SOMETHING

            // DELIVER
            if (target == -1) {
                IndexOfLastVersion(tempTxt);
                return target;
            }

            return target;
        }

        string ToVersionFormat(string version) {
            return $"## [{version}] - " + ToDateFormat();
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
                sb.AppendLine(value.srcLine);
                value.elementList.ForEach(ele => {
                    string tag = $"### {ele.tag}";
                    sb.Append(tag);
                    sb.Append(ele.content);
                });
            });
            return sb.ToString();
        }

        class VersionContainer {

            public const string TAG_PREFIX = "## ";

            public string version;
            public string srcLine;
            public List<VersionElement> elementList;

            public VersionContainer(string srcLine) {
                this.elementList = new List<VersionElement>();
                this.srcLine = srcLine;
            }

            public VersionElement AddElement(string tag, string content) {
                VersionElement ele = elementList.Find(value => value.tag == tag);
                if (ele == null) {
                    ele = new VersionElement();
                    ele.tag = tag;
                    elementList.Add(ele);
                }
                if (!string.IsNullOrEmpty(content) && !content.StartsWith("- ")) {
                    content = "- " + content;
                }
                ele.content += content + "\r\n";
                return ele;
            }

        }

        class VersionElement {

            public const string TAG_PREFIX = "### ";
            public const string TAG_ADDED = "Added";
            public const string TAG_CHANGED = "Changed";
            public const string TAG_REMOVED = "Removed";
            public const string TAG_FIXED = "Fixed";
            public const string TAG_OTHER = "Other";
            static List<string> tagList = new List<string> { TAG_ADDED, TAG_CHANGED, TAG_REMOVED, TAG_FIXED };

            public static bool StartsWithTag(string src, out string tag) {
                int index = tagList.FindIndex(value => src.StartsWith(value));
                if (index != -1) {
                    tag = tagList[index];
                    return true;
                } else {
                    tag = null;
                    return false;
                }
            }

            public string tag;
            public string content;

        }

    }

}
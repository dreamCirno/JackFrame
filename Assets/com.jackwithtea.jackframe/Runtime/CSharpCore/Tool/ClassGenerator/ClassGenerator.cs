using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace JackFrame {

    public enum VisitLevel {
        Public,
        Protected,
        Private,
        Internal,
    }

    public static class VisitLevelExtention {
        public static string GetString(this VisitLevel visitLevel) {
            string levelStr = visitLevel == VisitLevel.Private ? "" : visitLevel.ToString().ToLower() + " ";
            return levelStr;
        }
    }

    // 代码生成器
    // 类生成
    public class ClassGenerator {

        byte nestedCount;
        VisitLevel visitLevel;
        public string NamespaceName { get; private set; }
        public string ClassName { get; }
        List<string> UsingList { get; }
        List<FieldGenerator> fieldList;
        List<MethodGenerator> methodList;

        public ClassGenerator(VisitLevel visitLevel, string className, byte nestedCount = 0) {
            this.visitLevel = visitLevel;
            this.NamespaceName = string.Empty;
            this.ClassName = className;
            this.nestedCount = nestedCount;
            this.UsingList = new List<string>();
            this.fieldList = new List<FieldGenerator>();
            this.methodList = new List<MethodGenerator>();
        }

        public void SetNamespace(string namespaceName) {
            this.NamespaceName = namespaceName;
        }

        public void SetUsing(params string[] usingNameArr) {
            usingNameArr.ForeachWithoutNull(value => {
                string exist = UsingList.Find(u => u == value);
                if (string.IsNullOrEmpty(exist)) {
                    UsingList.Add($"using {value};");
                }
            });
        }

        public void SetField(VisitLevel visitLevel, string returnType, string fieldName) {
            fieldList.Add(new FieldGenerator(visitLevel, returnType, fieldName));
        }

        public void SetField(FieldGenerator fieldBox) {
            fieldList.Add(fieldBox);
        }

        public void SetMethod(MethodGenerator methodBox) {
            methodList.Add(methodBox);
        }

        public override string ToString() {

            StringBuilder sb = new StringBuilder();

            UsingList.ForEach(value => {
                sb.AppendLine(value);
            });

            sb.AppendLine();

            string nestedStr = "";

            // ---- Namespace Start ----
            if (!string.IsNullOrEmpty(NamespaceName)) {
                nestedCount = 0;
                nestedStr = "\t".Repeat(nestedCount);
                sb.AppendLine("namespace " + NamespaceName + @" {");
                sb.AppendLine();
            }

            // ---- Class Start ----
            nestedCount = 1;
            nestedStr = "\t".Repeat(nestedCount);
            sb.AppendLine($"{nestedStr}{visitLevel.GetString()}class {ClassName} {"{"}");
            sb.AppendLine();

            // ---- Fields And Methods ----
            nestedCount = 2;
            nestedStr = "\t".Repeat(nestedCount);
            fieldList.ForEach(value => {
                sb.AppendLine(nestedStr + value.ToString());
            });
            // sb.AppendLine();

            methodList.ForEach(value => {
                sb.AppendLine(value.GetString(nestedCount));
            });
            // sb.AppendLine();

            // ---- Class End ----
            nestedCount = 1;
            nestedStr = "\t".Repeat(nestedCount);
            sb.AppendLine($"{nestedStr}{"}"}");

            // ---- Namespace End ----
            nestedCount = 0;
            nestedStr = "\t".Repeat(nestedCount);
            if (!string.IsNullOrEmpty(NamespaceName)) {
                sb.AppendLine(@"}");
            }

            return sb.ToString();

        }

    }

    // 字段生成
    public class FieldGenerator {

        StringBuilder sb;
        public string FieldName { get; }

        public FieldGenerator(VisitLevel visitLevel, string returnType, string fieldName) {
            this.sb = new StringBuilder();
            this.FieldName = fieldName;
            sb.AppendLine($"{visitLevel.GetString()}{returnType} {fieldName};");
        }

        public override string ToString() {
            return sb.ToString();
        }

    }

    // 方法生成
    public class MethodGenerator {

        StringBuilder sb;
        string title;
        List<string> contentList;
        public string MethodName { get; }

        public MethodGenerator(VisitLevel level, string returnType, string methodName, params string[] args) {
            this.contentList = new List<string>();
            this.sb = new StringBuilder();
            this.MethodName = methodName;
            this.title = $"{level.GetString()}{returnType} {methodName}({args.ConnectInner(", ")}) {"{"}";
        }

        public void AppendLine(string content) {
            contentList.Add(content);
        }

        public string GetString(int nestedCount) {
            string nested = "\t".Repeat(nestedCount);
            sb.AppendLine($"{nested}{title}");
            nested = "\t".Repeat(nestedCount + 1);
            contentList.ForEach(value => {
                sb.AppendLine($"{nested}{value}");
            });
            nested = "\t".Repeat(nestedCount);
            sb.AppendLine($"{nested}{"}"}");
            return sb.ToString();
        }

    }

}
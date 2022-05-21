using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;


namespace JackFrame {
    public static class CsvReader {


        public static void CsvToClassFile(string[] csv, string namespaceName, string className, string outPath) {

            string classStr = "using System;\r\n";
            classStr += "using System.Collections.Generic;\r\n";
            if (namespaceName != "") {
                classStr += "\r\nnamespace " + namespaceName + " {\r\n";
            }
            classStr += "\r\n\tpublic class " + className + " {\r\n\r\n";

            string[] nameArray = csv[0].Split(',');
            string[] attributeArray = csv[1].Split(',');
            for (int i = 0; i < nameArray.Length; i++) {
                classStr += "\t\tpublic " + attributeArray[i] + " " + nameArray[i] + ";\r\n";
            }

            if (namespaceName != null) {
                classStr += "\r\n\t}\r\n";
            }
            classStr += "\r\n}";

            Console.WriteLine(classStr);

            try {
                File.WriteAllText(Path.Combine(outPath, className + ".cs"), classStr, Encoding.UTF8);
            }
            catch (Exception ex) {
                PLog.Error(ex.ToString());
            }
        }

        public static void CsvFileToClassFile(string path, string namespaceName, string className, string outPath) {
            try {
                string[] csvStr = File.ReadAllLines(path);
                CsvToClassFile(csvStr, namespaceName, className, outPath);
            }
            catch (Exception ex) {
                PLog.Error(ex.ToString());
            }
        }


        public static List<T> CsvFileToClassList<T>(string path, char arraySeparator) where T : class {
            try {
                string[] csvStr = File.ReadAllLines(path);
                return CsvToClassList<T>(csvStr, arraySeparator);
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
            return null;
        }


        public static List<T> CsvToClassList<T>(string[] csvStr, char arraySeparator) where T : class {

            List<T> tList = new List<T>();

            string[] strToClassTemp = new string[3];

            strToClassTemp[0] = csvStr[0];
            strToClassTemp[1] = csvStr[1];
            for (int i = 2; i < csvStr.Length; i++) {
                strToClassTemp[2] = csvStr[i];
                tList.Add(GetClass<T>(strToClassTemp, arraySeparator));
            }

            return tList;
        }

        /// <summary>
        /// 将二维数组转成一个类
        /// 第一行为字段名，确保csv表的第一行和类的字段一一对应，区分大小写
        /// 第二行为类型，大小写与C#中一致
        /// 第三行及之后为数据，bool用1表示true，用0表示false
        /// </summary>
        private static T GetClass<T>(string[] csvStrTemp, char arraySeparator) {
            Type t = typeof(T);
            FieldInfo[] fields = t.GetFields();
            T tempT = (T)Activator.CreateInstance(t);

            string[] nameArray = csvStrTemp[0].Split(',');
            string[] attributeArray = csvStrTemp[1].Split(',');
            string[] dataArray = csvStrTemp[2].Split(',');

            for (int i = 0; i < fields.Length; i++) {
                FieldInfo fieldInfo = fields[i];
                for (int j = 0; j < nameArray.Length; j++) {
                    if (fieldInfo.Name == nameArray[j]) {

                        string strTemp = dataArray[j];
                        switch (attributeArray[j]) {
                            case "byte":
                                byte.TryParse(strTemp, out byte byteTemp);
                                fieldInfo.SetValue(tempT, byteTemp);
                                break;
                            case "sbyte":
                                sbyte.TryParse(strTemp, out sbyte sbyteTemp);
                                fieldInfo.SetValue(tempT, sbyteTemp);
                                break;
                            case "short":
                                short.TryParse(strTemp, out short shortTemp);
                                fieldInfo.SetValue(tempT, shortTemp);
                                break;
                            case "ushort":
                                ushort.TryParse(strTemp, out ushort ushortTemp);
                                fieldInfo.SetValue(tempT, ushortTemp);
                                break;
                            case "int":
                                int.TryParse(strTemp, out int intTemp);
                                fieldInfo.SetValue(tempT, intTemp);
                                break;
                            case "uint":
                                uint.TryParse(strTemp, out uint uintTemp);
                                fieldInfo.SetValue(tempT, uintTemp);
                                break;
                            case "long":
                                long.TryParse(strTemp, out long longTemp);
                                fieldInfo.SetValue(tempT, longTemp);
                                break;
                            case "ulong":
                                ulong.TryParse(strTemp, out ulong ulongTemp);
                                fieldInfo.SetValue(tempT, ulongTemp);
                                break;
                            case "float":
                                float.TryParse(strTemp, out float floatTemp);
                                fieldInfo.SetValue(tempT, floatTemp);
                                break;
                            case "double":
                                double.TryParse(strTemp, out double doubleTemp);
                                fieldInfo.SetValue(tempT, doubleTemp);
                                break;
                            case "string":
                                fieldInfo.SetValue(tempT, strTemp);
                                break;
                            case "bool":
                                bool boolTemp = (strTemp == "1");
                                fieldInfo.SetValue(tempT, boolTemp);
                                break;
                            case "string[]":
                                //string[] strTemps = strTemp.Split(arraySeparator);
                                //fieldInfo.SetValue(tempT, strTemps);
                                fieldInfo.SetValue(tempT, GetArray<string>(strTemp, arraySeparator));
                                break;
                            case "int[]":
                                //string[] strTemp2 = strTemp.Split(arraySeparator);
                                //int[] intTemps = new int[strTemp2.Length];
                                //for (int intjishu = 0; intjishu < intTemps.Length; intjishu++) {
                                //    int.TryParse(strTemp2[intjishu], out intTemps[intjishu]);
                                //}
                                //fieldInfo.SetValue(tempT, intTemps);
                                fieldInfo.SetValue(tempT, GetArray<int>(strTemp, arraySeparator));
                                break;
                            case "bool[]":
                                fieldInfo.SetValue(tempT, GetArray<bool>(strTemp, arraySeparator));
                                break;
                            default:
                                PLog.Error("不支持类型: " + tempT);
                                break;
                        }
                        break;

                    }
                }
            }
            return tempT;
        }


        /// <summary>
        /// 字符串转数组
        /// </summary>
        private static T[] GetArray<T>(string str, char arraySeparator) {

            string[] strArrar = str.Split(arraySeparator);
            T[] tArrary = new T[strArrar.Length];

            for (int i = 0; i < tArrary.Length; i++) {
                tArrary[i] = ConvertTo<T>(strArrar[i]);
            }

            return tArrary;
        }

        /// <summary>
        /// 将string类型转成其他类型
        /// </summary>
        private static T ConvertTo<T>(string s) {
            object o = null;
            Type type = typeof(T);
            if (type == typeof(int)) {
                int.TryParse(s, out int i);
                o = i;
            } else if (type == typeof(float)) {
                float.TryParse(s, out float i);
                o = i;
            } else if (type == typeof(string)) {
                o = s;
            } else if (type == typeof(long)) {
                long.TryParse(s, out long i);
                o = i;
            } else if (type == typeof(bool)) {
                o = (s == "1");
            }
            return (T)o;
        }


        /// <summary>
        /// 某个字符在字符串中出现的次数
        /// </summary>
        private static int SubstringCount(string str, string substring) {
            if (str.Contains(substring)) {
                string strReplaced = str.Replace(substring, "");
                return (str.Length - strReplaced.Length) / substring.Length;
            }

            return 0;
        }

    }
}
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JackFrame {
    /*
        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系
    */
    public static class StringExtention {

        public static string WrapTextByLineSize(this string s, int lineSize) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i += lineSize) {
                if (lineSize > s.Length - i) {
                    lineSize = s.Length - i;
                }
                sb.AppendLine(s.Substring(i, lineSize));
            }
            return sb.ToString().TrimEnd();
        }

        public static string Repeat(this string s, int repeatTimes) {
            string tar = "";
            for (int i = 0; i < repeatTimes; i += 1) {
                tar += s;
            }
            return tar;
        }

        public static string ConnectInner(this string[] arr, string splitChar) {
            string str = "";
            for (int i = 0; i < arr.Length; i += 1) {
                string v = arr[i];
                if (i != arr.Length - 1) {
                    str += v + splitChar;
                } else {
                    str += v;
                }
            }
            return str;
        }

        public static T Parse<T>(this string s) {
            object o = null;
            Type type = typeof(T);
            if (type == typeof(int)) {
                int.TryParse(s, out int i);
                o = i;
            }
            else if (type == typeof(float)) {
                float.TryParse(s, out float i);
                o = i;
            }
            else if (type == typeof(string)) {
                o = s;
            }
            else if (type == typeof(long)) {
                long.TryParse(s, out long i);
                o = i;
            }
            return (T)o;
        }

        public static int Ip4ToInt(this string ip) {
            string[] arr = ip.Split('.');
            byte[] bytes = new byte[4];
            if (arr.Length != 4) {
                return -1;
            } else {
                for (int i = 0; i < arr.Length; i += 1) {
                    string s = arr[i];
                    byte.TryParse(s, out byte b);
                    bytes[i] = b;
                }
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        public static string ReplaceNumberToChinese(this string s) {
            return s.Replace('0', '零')
                .Replace('1', '一')
                .Replace('2', '二')
                .Replace('3', '三')
                .Replace('4', '四')
                .Replace('5', '五')
                .Replace('6', '六')
                .Replace('7', '七')
                .Replace('8', '八')
                .Replace('9', '九');
        }

        public static MatchCollection GetEnglishAndNum(this string str) {
            Regex reg = new Regex(@"[a-zA-Z0-9]");
            return reg.Matches(str);
        }

        public static MatchCollection GetMatchesIntegerBetweenTowChar(this string str, string startChar, string endChar) {
            Regex reg = new Regex(@"[" + startChar + "]+[0-9]+[" + endChar + "]");
            return reg.Matches(str);
        }

        public static MatchCollection GetMatchesLettersBetweenTwoChar(this string str, string startChar, string endChar) {
            Regex reg = new Regex(@"[" + startChar + "]+[a-zA-Z0-9.]+[" + endChar + "]");
            return reg.Matches(str);
        }

    }
}
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace JackFrame {

    public static class IniHelper {

        /// <summary>
        /// 为INI文件中指定的节点取得字符串
        /// </summary>
        /// <param name="lpAppName">欲在其中查找关键字的节点名称</param>
        /// <param name="lpKeyName">欲获取的项名</param>
        /// <param name="lpDefault">指定的项没有找到时返回的默认值</param>
        /// <param name="lpReturnedString">指定一个字串缓冲区，长度至少为nSize</param>
        /// <param name="nSize">指定装载到lpReturnedString缓冲区的最大字符数量</param>
        /// <param name="lpFileName">INI文件完整路径</param>
        /// <returns>复制到lpReturnedString缓冲区的字节数量，其中不包括那些NULL中止字符</returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        /// <summary>
        /// 修改INI文件中内容
        /// </summary>
        /// <param name="lpApplicationName">欲在其中写入的节点名称</param>
        /// <param name="lpKeyName">欲设置的项名</param>
        /// <param name="lpString">要写入的新字符串</param>
        /// <param name="lpFileName">INI文件完整路径</param>
        /// <returns>非零表示成功，零表示失败</returns>
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

        public static string ReadValue(string section, string key, string filePath) {
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, string.Empty, sb, 1024, filePath);
            return sb.ToString();
        }

        public static int WriteValue(string section, string key, string value, string filePath) {
            CreateFileIfNotExist(filePath);
            return WritePrivateProfileString(section, key, value, filePath);
        }

        static void CreateFileIfNotExist(string filePath) {
            if (!File.Exists(filePath)) {
                using (FileStream fs = File.Create(filePath)) {

                }
            }
        }

        public static void AddAndSave(string filePath, string key, string value) {

            CreateFileIfNotExist(filePath);

            string[] text = File.ReadAllLines(filePath);
            string[] newText = new string[text.Length + 1];
            text.CopyTo(newText, 0);
            newText[text.Length] = key + ":" + value;
            File.WriteAllLines(filePath, newText);
        }

        public static string Get(string filePath, string key) {

            if (!File.Exists(filePath)) {
                return string.Empty;
            }

            string[] text = File.ReadAllLines(filePath);
            for (int i = 0; i < text.Length; i += 1) {
                string str = text[i];
                string[] arr = str.Split(':');
                if (arr.Length < 2) {
                    continue;
                }
                string k = arr[0];
                if (k.Trim() == key) {
                    return arr[1];
                }
            }
            return string.Empty;
        }

    }
}
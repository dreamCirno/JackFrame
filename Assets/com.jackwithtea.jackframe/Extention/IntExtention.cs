using System;

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
    public static class IntExtention {

        public static bool Between(this int i, int min, int max) {
            return i >= min && i <= max;
        }

        public static int NextListIndex(this int index, int offAxis, int min, int max) {
            if (index + offAxis > max - 1) {
                return min;
            } else if (index + offAxis < 0) {
                return max - 1;
            } else {
                return index + offAxis;
            }
        }

        public static string Ip4ToString(this int ipInt) {
            byte[] bytes = BitConverter.GetBytes(ipInt);
            string s = "";
            for (int i = 0; i < bytes.Length; i += 1) {
                byte b = bytes[i];
                s += b.ToString();
                if (i != bytes.Length - 1) {
                    s += ".";
                }
            }
            return s;
        }

        public static string ToStringChinese(this int i) {
            return i.ToString().Replace('0', '零')
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

    }

}
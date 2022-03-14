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
    public static class DateExtention {

        public static string ToFileName(this DateTime t) {
            return t.Year.ToString()
                 + t.Month.ToString().PadLeft(2, '0')
                 + t.Day.ToString().PadLeft(2, '0')
                 + "_"
                 + t.Hour.ToString().PadLeft(2, '0')
                 + ","
                 + t.Minute.ToString().PadLeft(2, '0')
                 + ","
                 + t.Second.ToString().PadLeft(2, '0');
        }

        public static long GetTimeStamp(this DateTime t) {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan ts = DateTime.UtcNow - start;
            long a = Convert.ToInt64(ts.TotalMilliseconds);
            return a;
        }

        public static string ToYYYYMMDD(this DateTime t, char splitChar = '-') {
            return t.Year.ToString() + splitChar + t.Month.ToString().PadLeft(2, '0') + splitChar + t.Day.ToString().PadLeft(2, '0');
        }

    }
}
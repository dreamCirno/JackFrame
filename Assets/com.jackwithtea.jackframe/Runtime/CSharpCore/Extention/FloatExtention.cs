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
    public static class FloatExtention {

        public static float ToOne(this float f) {
            if (f > 0) {
                f = 1;
            } else if (f < 0) {
                f = -1;
            }
            return f;
        }

        public static float FloorToHalf(this float f) {
            float t = (float)Math.Floor(f);
            if (f > t + 0.5f) {
                return t + 0.5f;
            } else if (f < t + 0.5f) {
                return t;
            } else {
                return t;
            }
        }

        public static bool IsBetween(this float f, float targetValue, float range) {
            return f >= targetValue - range && f <= targetValue + range;
        }

        // 返回 mm:ss 格式
        public static string SecondToMMSS(this float leftSecond) {
            int m = (int)Math.Floor(leftSecond / 60f);
            int s = (int)Math.Ceiling(leftSecond % 60);
            return (m > 9 ? m.ToString() : ("0" + m)) + ":" + (s > 9 ? s.ToString() : ("0" + s));
        }

    }
}
using System;

namespace JackFrame {

    public static class NumberExtention {

        public static string ToBinaryString(this byte i) {
            return Convert.ToString(i, 2);
        }

        public static string ToBinaryString(this sbyte i) {
            return Convert.ToString(i, 2);
        }

        public static string ToBinaryString(this short i) {
            return Convert.ToString(i, 2);
        }

        public static string ToBinaryString(this ushort i) {
            return Convert.ToString(i, 2);
        }

        public static string ToBinaryString(this int i) {
            return Convert.ToString(i, 2);
        }

        public static string ToBinaryString(this uint i) {
            return Convert.ToString(i, 2);
        }

        public static string ToBinaryString(this long i) {
            return Convert.ToString(i, 2);
        }

        public static string ToBinaryString(this ulong i) {
            return Convert.ToString((long)i, 2);
        }

    }

}
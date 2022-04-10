using System;

namespace JackFrame {

    public static class IntHelper {

        public static ushort UInt8x2Compress(byte v1, byte v2) {
            ushort value = (ushort)v1;
            value |= (ushort)(v2 << 8);
            return value;
        }

        public static (byte v1, byte v2) UInt8x2Uncompress(ushort v) {
            byte lt = (byte)v;
            byte mt = (byte)(v >> 8);
            return (lt, mt);
        }

        public static uint UInt16x2Compress(ushort v1, ushort v2) {
            uint value = (uint)v1;
            value |= (uint)(v2 << 16);
            return value;
        }

        public static (ushort v1, ushort v2) UInt16x2Uncompress(uint v) {
            ushort lt = (ushort)v;
            ushort mt = (ushort)(v >> 16);
            return (lt, mt);
        }

        public static ulong UInt32x2Compress(uint v1, uint v2) {
            ulong value = v1;
            value |= v2 << 32;
            return value;
        }

        public static (uint v1, uint v2) UInt32x2Uncompress(ulong v) {
            uint lt = (uint)v;
            uint mt = (uint)v >> 32;
            return (lt, mt);
        }

    }

}
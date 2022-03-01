using System;
using JackBuffer;

namespace JackBuffer.Sample {

    [JackMessageObject]
    public struct MyModel {

        public char charValue;
        public byte byteValue;
        public sbyte sbyteValue;
        public short shortValue;
        public ushort ushortValue;
        public int intValue;
        public uint uintValue;
        public long longValue;
        public ulong ulongValue;
        public float floatValue;
        public double doubleValue;

        public byte[] byteArr;
        public sbyte[] sbyteArr;
        public short[] shortArr;
        public ushort[] ushortArr;
        public int[] intArr;
        public uint[] uintArr;
        public long[] longArr;
        public ulong[] ulongArr;
        public float[] floatArr;
        public double[] doubleArr;

        public string strValue;
        public string[] strArr;
        public HerModel herModel;

        // 自动生成
        public void WriteTo(byte[] dst, ref int offset) {
            BufferWriter.WriteChar(dst, charValue, ref offset);
        }

        // 自动生成
        public byte[] ToBytes() {
            bool isCertain = GetEvaluatedSize(out int count);
            byte[] src = new byte[count];
            int offset = 0;
            WriteTo(src, ref offset);
            if (isCertain) {
                return src;
            } else {
                byte[] dst = new byte[offset];
                Buffer.BlockCopy(src, 0, dst, 0, offset);
                return dst;
            }
        }

        // 自动生成
        bool GetEvaluatedSize(out int count) {
            bool isCertain = true;
            // 确定长度 + 字符串预估长度 + 自定义类型长度
            count = 1 + 2;

            // 是否确定的长度
            // 如果有字符串或自定义类型, 返回 false
            // 否则, 返回 true
            return isCertain;
        }

        // 自动生成
        public void FromBytes(byte[] src, ref int offset) {
            charValue = BufferReader.ReadChar(src, ref offset);
        }

    }

}
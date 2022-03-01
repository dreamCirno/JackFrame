using System;
using JackBuffer;

namespace JackBuffer.Sample {

    [JackMessageObject]
    public class HerModel {

        // 自动生成
        public void WriteTo(byte[] dst, ref int offset) {
        }

        // 自动生成
        public byte[] ToBytes() {
            bool isCertain = GetMaxSize(out int count);
            byte[] src = new byte[count];
            int offset = 0;
            WriteTo(src, ref offset);
            if (isCertain) {

            }
            byte[] dst = new byte[offset];
            Buffer.BlockCopy(src, 0, dst, 0, offset);
            return dst;
        }

        // 自动生成
        bool GetMaxSize(out int count) {
            bool isCertain = true;
            // 确定长度 + 字符串预估长度 + 自定义类型长度
            count = 1 + 2;

            // 是否确定的长度
            // 如果有字符串或自定义类型, 返回 false
            // 否则, 返回 true
            return isCertain;
        }

    }

}
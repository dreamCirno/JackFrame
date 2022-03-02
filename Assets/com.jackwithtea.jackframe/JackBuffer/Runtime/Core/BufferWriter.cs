using System;
using System.IO;
using System.Buffers;
using System.Text;
using System.Runtime.InteropServices;

namespace JackBuffer {

    public static class BufferWriter {

        public static void WriteInt8(byte[] dst, sbyte data, ref int offset) {
            WriteUInt8(dst, (byte)data, ref offset);
        }

        public static void WriteUInt8(byte[] dst, byte data, ref int offset) {
            dst[offset] = data;
            offset += 1;
        }

        public static void WriteChar(byte[] dst, char data, ref int offset) {
            dst[offset] = (byte)data;
            offset += 1;
            dst[offset] = (byte)(data >> 8);
            offset += 1;
        }

        public static void WriteInt16(byte[] dst, short data, ref int offset) {
            WriteUInt16(dst, (ushort)data, ref offset);
        }

        public static void WriteUInt16(byte[] dst, ushort data, ref int offset) {
            for (byte i = 0; i < 2; i += 1) {
                dst[offset] = (byte)(data >> (i * 8));
                offset += 1;
            }
        }

        public static void WriteSingle(byte[] dst, float data, ref int offset) {
            FloatContent content = new FloatContent();
            content.floatValue = data;
            WriteUInt32(dst, content.uintValue, ref offset);
        }

        public static void WriteInt32(byte[] dst, int data, ref int offset) {
            WriteUInt32(dst, (uint)data, ref offset);
        }

        public static void WriteUInt32(byte[] dst, uint data, ref int offset) {
            for (byte i = 0; i < 4; i += 1) {
                dst[offset] = (byte)(data >> (i * 8));
                offset += 1;
            }
        }

        public static void WriteDouble(byte[] dst, double data, ref int offset) {
            DoubleContent content = new DoubleContent();
            content.doubleValue = data;
            WriteUInt64(dst, content.ulongValue, ref offset);
        }

        public static void WriteInt64(byte[] dst, long data, ref int offset) {
            WriteUInt64(dst, (ulong)data, ref offset);
        }

        public static void WriteUInt64(byte[] dst, ulong data, ref int offset) {
            for (byte i = 0; i < 8; i += 1) {
                dst[offset] = (byte)(data >> (i * 8));
                offset += 1;
            }
        }

        public static void WriteUTF8String(byte[] dst, string data, ref int offset) {
            if (data != null) {
                byte[] d = Encoding.UTF8.GetBytes(data);
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                Buffer.BlockCopy(d, 0, dst, offset, count);
                offset += count;
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        static byte[] temp = new byte[4096];
        public static void WriteUTF8StringArr(byte[] dst, string[] data, ref int offset) {
            if (data != null) {
                int curOffset = 0;
                for (int i = 0; i < data.Length; i += 1) {
                    WriteUTF8String(temp, data[i], ref curOffset);
                }
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                Buffer.BlockCopy(temp, 0, dst, offset, curOffset);
                offset += curOffset;
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteMessage<T>(byte[] dst, IJackMessage<T> data, ref int offset) {
            if (data != null) {
                byte[] b = data.ToBytes();
                ushort count = (ushort)b.Length;
                WriteUInt16(dst, count, ref offset);
                WriteUint8Arr(dst, b, ref offset);
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteInt8Arr(byte[] dst, sbyte[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                Buffer.BlockCopy(data, 0, dst, offset, count);
                offset += count;
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteUint8Arr(byte[] dst, byte[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                Buffer.BlockCopy(data, 0, dst, offset, count);
                offset += count;
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteInt16Arr(byte[] dst, short[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                for (int i = 0; i < count; i += 1) {
                    WriteInt16(dst, data[i], ref offset);
                }
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteUInt16Arr(byte[] dst, ushort[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                for (int i = 0; i < count; i += 1) {
                    WriteUInt16(dst, data[i], ref offset);
                }
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteInt32Arr(byte[] dst, int[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                for (int i = 0; i < count; i += 1) {
                    WriteInt32(dst, data[i], ref offset);
                }
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteUInt32Arr(byte[] dst, uint[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                for (int i = 0; i < count; i += 1) {
                    WriteUInt32(dst, data[i], ref offset);
                }
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteSingleArr(byte[] dst, float[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                for (int i = 0; i < count; i += 1) {
                    WriteSingle(dst, data[i], ref offset);
                }
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteInt64Arr(byte[] dst, long[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                for (int i = 0; i < count; i += 1) {
                    WriteInt64(dst, data[i], ref offset);
                }
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteUInt64Arr(byte[] dst, ulong[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                for (int i = 0; i < count; i += 1) {
                    WriteUInt64(dst, data[i], ref offset);
                }
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

        public static void WriteDoubleArr(byte[] dst, double[] data, ref int offset) {
            if (data != null) {
                ushort count = (ushort)data.Length;
                WriteUInt16(dst, count, ref offset);
                for (int i = 0; i < count; i += 1) {
                    WriteDouble(dst, data[i], ref offset);
                }
            } else {
                WriteUInt16(dst, 0, ref offset);
            }
        }

    }

}
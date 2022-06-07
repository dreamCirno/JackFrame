using System;
using System.Runtime.CompilerServices;

namespace JackFrame.DefiniteMath {

    public struct Fixed64 {

        long raw;
        public long Raw => raw;

        // ==== CONSTANT ====
        public const int FRACTION_BITS = 32;
        public const int NUM_BITS = 64;
        public const long MIN_LONG = long.MinValue;
        public const long MAX_LONG = long.MaxValue;
        public readonly static Fixed64 MinFixed64 = M_FromRaw(long.MinValue);
        public readonly static Fixed64 MaxFixed64 = M_FromRaw(long.MaxValue);
        public const long ONE = 1L << FRACTION_BITS;

        // CTOR
        public Fixed64(int value) {
            this.raw = value * ONE;
        }

        public Fixed64(long value) {
            this.raw = value * ONE;
        }

        static Fixed64 M_FromRaw(long raw) {
            Fixed64 result = new Fixed64();
            result.raw = raw;
            return result;
        }

        public static Fixed64 FromRaw(long raw) {
            return M_FromRaw(raw);
        }

        // ==== OPERATOR ====
        public static Fixed64 operator +(Fixed64 a, Fixed64 b) {
            return M_FromRaw(a.raw + b.raw);
        }

        public static Fixed64 operator -(Fixed64 a, Fixed64 b) {
            return M_FromRaw(a.raw - b.raw);
        }

        public static Fixed64 operator *(Fixed64 a, Fixed64 b) {

            long ar = a.raw;
            long br = b.raw;

            ulong alow = (ulong)ar & 0xFFFFFFFF;
            long ahigh = ar >> FRACTION_BITS;
            ulong blow = (ulong)br & 0xFFFFFFFF;
            long bhigh = br >> FRACTION_BITS;

            ulong lowlow = alow * blow;
            long lowhigh = (long)alow * bhigh;
            long highlow = ahigh * (long)blow;
            long highhigh = ahigh * bhigh;

            ulong low = lowlow >> FRACTION_BITS;
            long high = highhigh << FRACTION_BITS;

            long result = (long)low + lowhigh + highlow + high;
            return M_FromRaw(result);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int CountLeadingZeroes(ulong x) {
            int result = 0;
            while ((x & 0xF000_0000_0000_0000) == 0) { result += 4; x <<= 4; }
            while ((x & 0x8000_0000_0000_0000) == 0) { result += 1; x <<= 1; }
            return result;
        }

        public static Fixed64 operator /(Fixed64 a, Fixed64 b) {

            long ar = a.raw;
            long br = b.raw;

            if (br == 0) {
                throw new DivideByZeroException();
            }

            // 被除数(余数)
            ulong remainder = (ulong)(ar >= 0 ? ar : -ar);

            // 除数
            ulong divider = (ulong)(br >= 0 ? br : -br);

            // 商
            ulong quotient = 0UL;

            // 被除数可用的位数
            int bitPos = FRACTION_BITS + 1;

            // 此举是优化, 把除数低位非0的数据直接移位运算
            while ((divider & 0xF) == 0 && bitPos >= 4) {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos > 0) {
                // 此举是优化, 把被除数高位非0的数据直接移位运算
                int leadingZeroShift = CountLeadingZeroes(remainder);
                if (leadingZeroShift > bitPos) {
                    leadingZeroShift = bitPos;
                }
                remainder <<= leadingZeroShift;
                bitPos -= leadingZeroShift;

                // 不带余数的商
                ulong divRes = remainder / divider;

                // 只余数
                remainder = remainder % divider;

                // 商加在整数位
                quotient += divRes << bitPos;

                if ((divRes & ~(0xFFFF_FFFF_FFFF_FFFF >> bitPos)) != 0) {
                    return ((ar ^ br) & MIN_LONG) == 0 ? MaxFixed64 : MinFixed64;
                }

                remainder <<= 1;
                bitPos -= 1;

            }

            // ROUND
            long res = (long)((quotient + 1) >> 1);
            if (((ar ^ br) & MIN_LONG) != 0) {
                res = -res;
            }

            return M_FromRaw(res);

        }

        public static Fixed64 operator %(Fixed64 a, Fixed64 b) {
            return M_FromRaw(a.raw % b.raw);
        }

        public static bool operator ==(Fixed64 a, Fixed64 b) {
            return a.raw == b.raw;
        }

        public static bool operator !=(Fixed64 a, Fixed64 b) {
            return a.raw != b.raw;
        }

        public static bool operator >(Fixed64 a, Fixed64 b) {
            return a.raw > b.raw;
        }

        public static bool operator <(Fixed64 a, Fixed64 b) {
            return a.raw < b.raw;
        }

        public static bool operator >=(Fixed64 a, Fixed64 b) {
            return a.raw >= b.raw;
        }

        public static bool operator <=(Fixed64 a, Fixed64 b) {
            return a.raw <= b.raw;
        }

        // ==== IMPLICIT / EXPLICIT ====
        // - long
        public static implicit operator Fixed64(long value) {
            return M_FromRaw(value);
        }

        public static explicit operator long(Fixed64 value) {
            return value.raw >> FRACTION_BITS;
        }

        // - int
        public static explicit operator int(Fixed64 value) {
            return (int)(value.raw >> FRACTION_BITS);
        }

        public static implicit operator Fixed64(int value) {
            return M_FromRaw(value);
        }

        // - float
        public static explicit operator float(Fixed64 value) {
            return (float)value.raw / ONE;
        }

        public static explicit operator Fixed64(float value) {
            return M_FromRaw((long)(value * ONE));
        }

        // - double
        public static explicit operator double(Fixed64 value) {
            return (double)value.raw / ONE;
        }

        public static explicit operator Fixed64(double value) {
            return M_FromRaw((long)(value * ONE));
        }

        public static explicit operator decimal(Fixed64 value) {
            return (decimal)value.raw / ONE;
        }

        // ==== PARSER ====
        public int ToInt() {
            return (int)(raw >> FRACTION_BITS);
        }

        public long ToLong() {
            return raw >> FRACTION_BITS;
        }

        public float ToFloat() {
            return (float)raw / ONE;
        }

        // ==== EXTENTION ====

        // ==== OVERRIDE ====
        public override bool Equals(object obj) {
            if (obj is Fixed64) {
                return this.raw == ((Fixed64)obj).raw;
            }
            return false;
        }

        public bool Equals(Fixed64 other) {
            return this.raw == other.raw;
        }

        public override int GetHashCode() {
            return raw.GetHashCode();
        }

    }

}
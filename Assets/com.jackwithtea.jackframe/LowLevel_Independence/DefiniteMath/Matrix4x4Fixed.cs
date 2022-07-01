using System;

namespace JackFrame.DefiniteMath {

    public struct Matrix4x4Fixed {

        public Fixed64 m11, m12, m13, m14;
        public Fixed64 m21, m22, m23, m24;
        public Fixed64 m31, m32, m33, m34;
        public Fixed64 m41, m42, m43, m44;

        public readonly static Matrix4x4Fixed identity = new Matrix4x4Fixed(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );

        public Matrix4x4Fixed(Vector4Fixed row1, Vector4Fixed row2, Vector4Fixed row3, Vector4Fixed row4) {
            this.m11 = row1.x; this.m12 = row1.y; this.m13 = row1.z; this.m14 = row1.w;
            this.m21 = row2.x; this.m22 = row2.y; this.m23 = row2.z; this.m24 = row2.w;
            this.m31 = row3.x; this.m32 = row3.y; this.m33 = row3.z; this.m34 = row3.w;
            this.m41 = row4.x; this.m42 = row4.y; this.m43 = row4.z; this.m44 = row4.w;
        }

        public Matrix4x4Fixed(
            Fixed64 m11, Fixed64 m12, Fixed64 m13, Fixed64 m14,
            Fixed64 m21, Fixed64 m22, Fixed64 m23, Fixed64 m24,
            Fixed64 m31, Fixed64 m32, Fixed64 m33, Fixed64 m34,
            Fixed64 m41, Fixed64 m42, Fixed64 m43, Fixed64 m44
        ) {
            this.m11 = m11; this.m12 = m12; this.m13 = m13; this.m14 = m14;
            this.m21 = m21; this.m22 = m22; this.m23 = m23; this.m24 = m24;
            this.m31 = m31; this.m32 = m32; this.m33 = m33; this.m34 = m34;
            this.m41 = m41; this.m42 = m42; this.m43 = m43; this.m44 = m44;
        }

        public override string ToString() {
            return $"{m11} {m12} {m13} {m14}\r\n"
                  + $"{m21} {m22} {m23} {m24}\r\n"
                  + $"{m31} {m32} {m33} {m34}\r\n"
                  + $"{m41} {m42} {m43} {m44}";
        }

        public static Matrix4x4Fixed ScaleByVector(Vector3Fixed scales) {
            return Scale(scales.x, scales.y, scales.z);
        }

        public static Matrix4x4Fixed Scale(Fixed64 x, Fixed64 y, Fixed64 z) {
            Matrix4x4Fixed mt = new Matrix4x4Fixed(
                new Vector4Fixed(x, 0, 0, 0),
                new Vector4Fixed(0, y, 0, 0),
                new Vector4Fixed(0, 0, z, 0),
                new Vector4Fixed(0, 0, 0, 1)
            );
            return mt;
        }

        public static Matrix4x4Fixed Transpose(Matrix4x4Fixed value) {
            Fixed64 tmp = value.m12;
            value.m12 = value.m21;
            value.m21 = tmp;

            tmp = value.m13;
            value.m13 = value.m31;
            value.m31 = tmp;

            tmp = value.m14;
            value.m14 = value.m41;
            value.m41 = tmp;

            tmp = value.m23;
            value.m23 = value.m32;
            value.m32 = tmp;

            tmp = value.m24;
            value.m24 = value.m42;
            value.m42 = tmp;

            tmp = value.m34;
            value.m34 = value.m43;
            value.m43 = tmp;

            return value;
        }

        public static Matrix4x4Fixed TranslateByVector(Vector3Fixed translation) {
            return Translate(translation.x, translation.y, translation.z);
        }

        public static Matrix4x4Fixed Translate(Fixed64 x, Fixed64 y, Fixed64 z) {
            Matrix4x4Fixed mt = new Matrix4x4Fixed(
                new Vector4Fixed(1, 0, 0, x),
                new Vector4Fixed(0, 1, 0, y),
                new Vector4Fixed(0, 0, 1, z),
                new Vector4Fixed(0, 0, 0, 1)
            );
            return mt;
        }

        public static Matrix4x4Fixed Add(in Matrix4x4Fixed a, in Matrix4x4Fixed b) {
            return new Matrix4x4Fixed(
                a.m11 + b.m11, a.m12 + b.m12, a.m13 + b.m13, a.m14 + b.m14,
                a.m21 + b.m21, a.m22 + b.m22, a.m23 + b.m23, a.m24 + b.m24,
                a.m31 + b.m31, a.m32 + b.m32, a.m33 + b.m33, a.m34 + b.m34,
                a.m41 + b.m41, a.m42 + b.m42, a.m43 + b.m43, a.m44 + b.m44
            );
        }

        public static Matrix4x4Fixed Substract(in Matrix4x4Fixed lhs, in Matrix4x4Fixed rhs) {
            return new Matrix4x4Fixed(
                lhs.m11 - rhs.m11, lhs.m12 - rhs.m12, lhs.m13 - rhs.m13, lhs.m14 - rhs.m14,
                lhs.m21 - rhs.m21, lhs.m22 - rhs.m22, lhs.m23 - rhs.m23, lhs.m24 - rhs.m24,
                lhs.m31 - rhs.m31, lhs.m32 - rhs.m32, lhs.m33 - rhs.m33, lhs.m34 - rhs.m34,
                lhs.m41 - rhs.m41, lhs.m42 - rhs.m42, lhs.m43 - rhs.m43, lhs.m44 - rhs.m44
            );
        }

        public static Matrix4x4Fixed Multiply(in Matrix4x4Fixed lhs, in Matrix4x4Fixed rhs) {
            return new Matrix4x4Fixed(
                lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31 + lhs.m14 * rhs.m41,
                lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32 + lhs.m14 * rhs.m42,
                lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33 + lhs.m14 * rhs.m43,
                lhs.m11 * rhs.m14 + lhs.m12 * rhs.m24 + lhs.m13 * rhs.m34 + lhs.m14 * rhs.m44,

                lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31 + lhs.m24 * rhs.m41,
                lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32 + lhs.m24 * rhs.m42,
                lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33 + lhs.m24 * rhs.m43,
                lhs.m21 * rhs.m14 + lhs.m22 * rhs.m24 + lhs.m23 * rhs.m34 + lhs.m24 * rhs.m44,

                lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31 + lhs.m34 * rhs.m41,
                lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32 + lhs.m34 * rhs.m42,
                lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33 + lhs.m34 * rhs.m43,
                lhs.m31 * rhs.m14 + lhs.m32 * rhs.m24 + lhs.m33 * rhs.m34 + lhs.m34 * rhs.m44,

                lhs.m41 * rhs.m11 + lhs.m42 * rhs.m21 + lhs.m43 * rhs.m31 + lhs.m44 * rhs.m41,
                lhs.m41 * rhs.m12 + lhs.m42 * rhs.m22 + lhs.m43 * rhs.m32 + lhs.m44 * rhs.m42,
                lhs.m41 * rhs.m13 + lhs.m42 * rhs.m23 + lhs.m43 * rhs.m33 + lhs.m44 * rhs.m43,
                lhs.m41 * rhs.m14 + lhs.m42 * rhs.m24 + lhs.m43 * rhs.m34 + lhs.m44 * rhs.m44
            );
        }

        [Obsolete("Please Use Matrix4x4Fixed.Multiply")]
        public static Matrix4x4Fixed operator *(Matrix4x4Fixed lhs, Matrix4x4Fixed rhs) {
            Matrix4x4Fixed mt = new Matrix4x4Fixed(
                new Vector4Fixed(
                    lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31 + lhs.m14 * rhs.m41,
                    lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32 + lhs.m14 * rhs.m42,
                    lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33 + lhs.m14 * rhs.m43,
                    lhs.m11 * rhs.m14 + lhs.m12 * rhs.m24 + lhs.m13 * rhs.m34 + lhs.m14 * rhs.m44
                ),
                new Vector4Fixed(
                    lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31 + lhs.m24 * rhs.m41,
                    lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32 + lhs.m24 * rhs.m42,
                    lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33 + lhs.m24 * rhs.m43,
                    lhs.m21 * rhs.m14 + lhs.m22 * rhs.m24 + lhs.m23 * rhs.m34 + lhs.m24 * rhs.m44
                ),
                new Vector4Fixed(
                    lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31 + lhs.m34 * rhs.m41,
                    lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32 + lhs.m34 * rhs.m42,
                    lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33 + lhs.m34 * rhs.m43,
                    lhs.m31 * rhs.m14 + lhs.m32 * rhs.m24 + lhs.m33 * rhs.m34 + lhs.m34 * rhs.m44
                ),
                new Vector4Fixed(
                    lhs.m41 * rhs.m11 + lhs.m42 * rhs.m21 + lhs.m43 * rhs.m31 + lhs.m44 * rhs.m41,
                    lhs.m41 * rhs.m12 + lhs.m42 * rhs.m22 + lhs.m43 * rhs.m32 + lhs.m44 * rhs.m42,
                    lhs.m41 * rhs.m13 + lhs.m42 * rhs.m23 + lhs.m43 * rhs.m33 + lhs.m44 * rhs.m43,
                    lhs.m41 * rhs.m14 + lhs.m42 * rhs.m24 + lhs.m43 * rhs.m34 + lhs.m44 * rhs.m44
                )
            );
            return mt;
        }

        [Obsolete("Please Use Matrix4x4Fixed.Add")]
        public static Matrix4x4Fixed operator +(Matrix4x4Fixed a, Matrix4x4Fixed b) {
            a.m11 += b.m11; a.m12 += b.m12; a.m13 += b.m13; a.m14 += b.m14;
            a.m21 += b.m21; a.m22 += b.m22; a.m23 += b.m23; a.m24 += b.m24;
            a.m31 += b.m31; a.m32 += b.m32; a.m33 += b.m33; a.m34 += b.m34;
            a.m41 += b.m41; a.m42 += b.m42; a.m43 += b.m43; a.m44 += b.m44;
            return a;
        }

        [Obsolete("Please Use Matrix4x4Fixed.Substract")]
        public static Matrix4x4Fixed operator -(Matrix4x4Fixed lhs, Matrix4x4Fixed rhs) {
            lhs.m11 -= rhs.m11; lhs.m12 -= rhs.m12; lhs.m13 -= rhs.m13; lhs.m14 -= rhs.m14;
            lhs.m21 -= rhs.m21; lhs.m22 -= rhs.m22; lhs.m23 -= rhs.m23; lhs.m24 -= rhs.m24;
            lhs.m31 -= rhs.m31; lhs.m32 -= rhs.m32; lhs.m33 -= rhs.m33; lhs.m34 -= rhs.m34;
            lhs.m41 -= rhs.m41; lhs.m42 -= rhs.m42; lhs.m43 -= rhs.m43; lhs.m44 -= rhs.m44;
            return lhs;
        }

    }

}
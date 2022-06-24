using System;

namespace JackFrame.DefiniteMath {

    public struct Vector4Fixed {
        
        public Fixed64 x;
        public Fixed64 y;
        public Fixed64 z;
        public Fixed64 w;

        public Vector4Fixed(Fixed64 x, Fixed64 y, Fixed64 z, Fixed64 w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4Fixed(Vector4Fixed other) {
            this.x = other.x;
            this.y = other.y;
            this.z = other.z;
            this.w = other.w;
        }

        public static Vector4Fixed operator +(Vector4Fixed a, Vector4Fixed b) {
            a.x += b.x;
            a.y += b.y;
            a.z += b.z;
            a.w += b.w;
            return a;
        }

        public static Vector4Fixed operator -(Vector4Fixed lhs, Vector4Fixed rhs) {
            lhs.x -= rhs.x;
            lhs.y -= rhs.y;
            lhs.z -= rhs.z;
            lhs.w -= rhs.w;
            return lhs;
        }

    }
}
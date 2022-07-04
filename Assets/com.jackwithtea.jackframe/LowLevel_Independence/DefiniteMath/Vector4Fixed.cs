using System;
using FixMath.NET;

namespace JackFrame.DefiniteMath {

    public struct Vector4Fixed {
        
        public Fix64 x;
        public Fix64 y;
        public Fix64 z;
        public Fix64 w;

        public Vector4Fixed(Fix64 x, Fix64 y, Fix64 z, Fix64 w) {
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

        public override bool Equals(object obj) {
            var other = (Vector4Fixed)obj;
            return other == this;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
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

        public static bool operator ==(Vector4Fixed a, Vector4Fixed b) {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }

        public static bool operator !=(Vector4Fixed a, Vector4Fixed b) {
            return a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
        }

    }
}
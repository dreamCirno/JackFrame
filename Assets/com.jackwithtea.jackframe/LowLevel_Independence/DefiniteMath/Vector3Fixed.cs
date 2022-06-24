using System;

namespace JackFrame.DefiniteMath {

    public struct Vector3Fixed {

        public Fixed64 x;
        public Fixed64 y;
        public Fixed64 z;

        public Vector3Fixed(Fixed64 x, Fixed64 y, Fixed64 z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3Fixed(Vector3Fixed other) {
            this.x = other.x;
            this.y = other.y;
            this.z = other.z;
        }

        public override bool Equals(object obj) {
            var other = (Vector3Fixed)obj;
            return other == this;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static Vector3Fixed operator +(Vector3Fixed a, Vector3Fixed b) {
            a.x += b.x;
            a.y += b.y;
            a.z += b.z;
            return a;
        }

        public static Vector3Fixed operator -(Vector3Fixed lhs, Vector3Fixed rhs) {
            lhs.x -= rhs.x;
            lhs.y -= rhs.y;
            lhs.z -= rhs.z;
            return lhs;
        }

        public static bool operator ==(Vector3Fixed a, Vector3Fixed b) {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(Vector3Fixed a, Vector3Fixed b) {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }

    }
}
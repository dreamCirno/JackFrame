using System;

namespace JackFrame.DefiniteMath {

    public struct Vector2Fixed {
        
        public Fixed64 x;
        public Fixed64 y;

        public Vector2Fixed(Fixed64 x, Fixed64 y) {
            this.x = x;
            this.y = y;
        }

        public Vector2Fixed(Vector2Fixed other) {
            this.x = other.x;
            this.y = other.y;
        }

        public override bool Equals(object obj) {
            var other = (Vector2Fixed)obj;
            return other == this;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static Vector2Fixed operator +(Vector2Fixed a, Vector2Fixed b) {
            a.x += b.x;
            a.y += b.y;
            return a;
        }

        public static Vector2Fixed operator -(Vector2Fixed lhs, Vector2Fixed rhs) {
            lhs.x -= rhs.x;
            lhs.y -= rhs.y;
            return lhs;
        }

        public static bool operator ==(Vector2Fixed a, Vector2Fixed b) {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vector2Fixed a, Vector2Fixed b) {
            return a.x != b.x || a.y != b.y;
        }

    }
}
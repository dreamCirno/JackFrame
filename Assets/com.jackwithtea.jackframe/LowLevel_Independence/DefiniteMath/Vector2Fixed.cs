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

    }
}
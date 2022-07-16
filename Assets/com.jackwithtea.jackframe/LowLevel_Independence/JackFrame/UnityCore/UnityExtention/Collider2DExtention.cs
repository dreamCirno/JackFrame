using UnityEngine;

namespace JackFrame {

    public static class Collider2DExtention {

        public static Vector2 GetRightUpBorderPos(this Collider2D c) {
            SpriteRenderer sr = c.GetComponent<SpriteRenderer>();
            if (sr != null) {
                return (Vector2)c.transform.position + sr.size;
            } else {
                return (Vector2)c.transform.position + c.offset + (Vector2)c.bounds.size / 2;
            }
        }

        public static void SetAsBoxSize(this PolygonCollider2D polygon, float width, float height) {
            Vector2 leftDown = Vector2.zero;
            Vector2 leftUp = new Vector2(0, height);
            Vector2 rightUp = new Vector2(width, height);
            Vector2 rightDown = new Vector2(width, 0);
            polygon.SetPath(0, new Vector2[] { leftDown, leftUp, rightUp, rightDown });
        }

    }
}
using UnityEngine;
using BEPUPhysics1int.CollisionShapes.ConvexShapes;

namespace BEPUPhysics1int {

    public static class EntitiesExtention {

        public static void Editor_DrawGizmos(this Entity entity) {
#if UNITY_EDITOR

            Gizmos.color = Color.red;

            // Foot
            var footPos = entity.GetFootPosition().ToVector();
            Gizmos.DrawSphere(footPos, 0.1f);

            // Center
            var pos = entity.position.ToVector();
            var rot = entity.orientation.ToQuaternion();
            Gizmos.DrawSphere(pos, 0.1f);

            // Forward
            var fwd = entity.MotionState.WorldTransform.Forward;
            Gizmos.DrawLine(entity.position.ToVector(), (entity.position + fwd * 2).ToVector());

            // Shape
            Vector3 size = Vector3.zero;
            var shape = entity.CollisionInformation.shape;
            if (shape is SphereShape sphere) {
                size = Vector3.one * sphere.Radius.AsFloat();
            } else if (shape is BoxShape box) {
                size = new FixedV3(box.Width, box.Height, box.Length).ToVector();
            } else if (shape is CapsuleShape capsule) {
                size = Vector3.one * capsule.Radius.AsFloat();
            }

            Matrix4x4 cubeTransform = Matrix4x4.TRS(pos, rot, size);
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

            Gizmos.matrix *= cubeTransform;

            if (shape is SphereShape sphere1) {
                Gizmos.DrawWireSphere(Vector3.zero, 1);
            } else if (shape is BoxShape box1) {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            } else if (shape is CapsuleShape capsule1) {
                Gizmos.DrawWireSphere(Vector3.zero, 1);
                Gizmos.DrawWireSphere(new FixedV3(0, capsule1.Length / 2, 0).ToVector(), 1);
                Gizmos.DrawWireSphere(new FixedV3(0, -capsule1.Length / 2, 0).ToVector(), 1);
            }

            Gizmos.matrix = oldGizmosMatrix;

#endif
        }

    }
}
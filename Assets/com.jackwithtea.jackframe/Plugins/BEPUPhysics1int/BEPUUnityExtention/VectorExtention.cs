#if UNITY_2017_1_OR_NEWER
using UnityEngine;

namespace BEPUPhysics1int {

    public static class VectorExtention {

        public static Vector3 ToVector(this FixedV3 v) {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static Quaternion ToQuaternion(this FixedQuaternion v) {
            return new Quaternion((float)v.X, (float)v.Y, (float)v.Z, (float)v.W);
        }

    }

}
#endif
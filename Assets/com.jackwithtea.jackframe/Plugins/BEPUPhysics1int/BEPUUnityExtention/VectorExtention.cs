#if UNITY_2017_1_OR_NEWER
using UnityEngine;

namespace BEPUPhysics1int {

    public static class VectorExtention {

        public static Vector3 ToVector3(this FixedV3 v) {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }

    }

}
#endif
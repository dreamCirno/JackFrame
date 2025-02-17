﻿

using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.CollisionTests
{
    ///<summary>
    /// Extra data associated with a contact point used to refresh contacts each frame.
    ///</summary>
    public struct ContactSupplementData
    {
        /// <summary>
        /// Offset from the center of the first object to the contact point in the object's local space.
        /// </summary>
        public FixedV3 LocalOffsetA;

        /// <summary>
        /// Offset from the center of the second object to the contact point in the object's local space.
        /// </summary>
        public FixedV3 LocalOffsetB;
        /// <summary>
        /// Original penetration depth computed at the associatd contact.
        /// </summary>
        public Fixed64 BasePenetrationDepth;
    }
}

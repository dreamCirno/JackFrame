﻿using BEPUPhysics1int.CollisionTests.Manifolds;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a terrain-convex collision pair.
    ///</summary>
    public sealed class TerrainConvexPairHandler : TerrainPairHandler
    {
        private TerrainConvexContactManifold contactManifold = new TerrainConvexContactManifold();
        protected override TerrainContactManifold TerrainManifold
        {
            get { return contactManifold; }
        }

    }

}

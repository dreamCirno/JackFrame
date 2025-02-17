﻿using BEPUPhysics1int.CollisionTests.Manifolds;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a terrain-sphere collision pair.
    ///</summary>
    public sealed class TerrainSpherePairHandler : TerrainPairHandler
    {
        private TerrainSphereContactManifold contactManifold = new TerrainSphereContactManifold();
        protected override TerrainContactManifold TerrainManifold
        {
            get { return contactManifold; }
        }

    }

}

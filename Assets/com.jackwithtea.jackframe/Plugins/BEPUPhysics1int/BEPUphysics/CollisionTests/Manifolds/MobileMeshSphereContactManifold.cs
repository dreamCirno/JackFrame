﻿using BEPUPhysics1int.CollisionTests.CollisionAlgorithms;
using BEPUPhysics1int.ResourceManagement;

namespace BEPUPhysics1int.CollisionTests.Manifolds
{
    ///<summary>
    /// Manages persistent contacts between a convex and an instanced mesh.
    ///</summary>
    public class MobileMeshSphereContactManifold : MobileMeshContactManifold
    {

        UnsafeResourcePool<TriangleSpherePairTester> testerPool = new UnsafeResourcePool<TriangleSpherePairTester>();
        protected override void GiveBackTester(TrianglePairTester tester)
        {
            testerPool.GiveBack((TriangleSpherePairTester)tester);
        }

        protected override TrianglePairTester GetTester()
        {
            return testerPool.Take();
        }

    }
}

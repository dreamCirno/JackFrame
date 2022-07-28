using BEPUPhysics1int.CollisionTests.CollisionAlgorithms;
using BEPUPhysics1int.ResourceManagement;

namespace BEPUPhysics1int.CollisionTests.Manifolds
{
    public class TerrainConvexContactManifold : TerrainContactManifold
    {
        static LockingResourcePool<TriangleConvexPairTester> testerPool = new LockingResourcePool<TriangleConvexPairTester>();
        protected override TrianglePairTester GetTester()
        {
            return testerPool.Take();
        }

        protected override void GiveBackTester(TrianglePairTester tester)
        {
            testerPool.GiveBack((TriangleConvexPairTester)tester);
        }

    }
}

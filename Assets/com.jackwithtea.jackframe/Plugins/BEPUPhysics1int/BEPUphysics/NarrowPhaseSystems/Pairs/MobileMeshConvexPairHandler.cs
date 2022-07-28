using BEPUPhysics1int.CollisionTests.Manifolds;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a mobile mesh-convex collision pair.
    ///</summary>
    public class MobileMeshConvexPairHandler : MobileMeshPairHandler
    {
        MobileMeshConvexContactManifold contactManifold = new MobileMeshConvexContactManifold();
        protected internal override MobileMeshContactManifold MeshManifold
        {
            get { return contactManifold; }
        }



    }

}

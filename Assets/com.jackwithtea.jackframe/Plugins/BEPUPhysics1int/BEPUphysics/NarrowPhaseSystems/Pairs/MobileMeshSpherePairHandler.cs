using BEPUPhysics1int.CollisionTests.Manifolds;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a mobile mesh-sphere collision pair.
    ///</summary>
    public class MobileMeshSpherePairHandler : MobileMeshPairHandler
    {
        MobileMeshSphereContactManifold contactManifold = new MobileMeshSphereContactManifold();
        protected internal override MobileMeshContactManifold MeshManifold
        {
            get { return contactManifold; }
        }



    }

}

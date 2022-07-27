using BEPUPhysics1int.CollisionTests.Manifolds;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a instanced mesh-convex collision pair.
    ///</summary>
    public class InstancedMeshConvexPairHandler : InstancedMeshPairHandler
    {

        InstancedMeshConvexContactManifold contactManifold = new InstancedMeshConvexContactManifold();
        protected override InstancedMeshContactManifold MeshManifold
        {
            get { return contactManifold; }
        }
        

    }

}

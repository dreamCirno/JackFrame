﻿using BEPUPhysics1int.BroadPhaseEntries;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;
using BEPUPhysics1int.ResourceManagement;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a mobile mesh-mobile mesh collision pair.
    ///</summary>
    public class MobileMeshMobileMeshPairHandler : MobileMeshMeshPairHandler
    {


        MobileMeshCollidable mesh;

        public override Collidable CollidableB
        {
            get { return mesh; }
        }
        public override Entity EntityB
        {
            get { return mesh.entity; }
        }
        protected override Materials.Material MaterialB
        {
            get { return mesh.entity.material; }
        }

        protected override TriangleCollidable GetOpposingCollidable(int index)
        {
            //Construct a TriangleCollidable from the static mesh.
            var toReturn = PhysicsResources.GetTriangleCollidable();
            toReturn.Shape.sidedness = mesh.Shape.Sidedness;
            toReturn.Shape.collisionMargin = mobileMesh.Shape.MeshCollisionMargin;
            toReturn.Entity = mesh.entity;
            return toReturn;
        }

        protected override void CleanUpCollidable(TriangleCollidable collidable)
        {
            collidable.Entity = null;
            base.CleanUpCollidable(collidable);
        }

        protected override void ConfigureCollidable(TriangleEntry entry, Fixed64 dt)
        {
            var shape = entry.Collidable.Shape;
            mesh.Shape.TriangleMesh.Data.GetTriangle(entry.Index, out shape.vA, out shape.vB, out shape.vC);
            BEPUMatrix3x3 o;
            BEPUMatrix3x3.CreateFromQuaternion(ref mesh.worldTransform.Orientation, out o);
            BEPUMatrix3x3.Transform(ref shape.vA, ref o, out shape.vA);
            BEPUMatrix3x3.Transform(ref shape.vB, ref o, out shape.vB);
            BEPUMatrix3x3.Transform(ref shape.vC, ref o, out shape.vC);
            FixedV3 center;
            FixedV3.Add(ref shape.vA, ref shape.vB, out center);
            FixedV3.Add(ref center, ref shape.vC, out center);
            FixedV3.Multiply(ref center, F64.OneThird, out center);
            FixedV3.Subtract(ref shape.vA, ref center, out shape.vA);
            FixedV3.Subtract(ref shape.vB, ref center, out shape.vB);
            FixedV3.Subtract(ref shape.vC, ref center, out shape.vC);

            FixedV3.Add(ref center, ref mesh.worldTransform.Position, out center);
            //The bounding box doesn't update by itself.
            entry.Collidable.worldTransform.Position = center;
            entry.Collidable.worldTransform.Orientation = FixedQuaternion.Identity;
            entry.Collidable.UpdateBoundingBoxInternal(dt);
        }

        ///<summary>
        /// Initializes the pair handler.
        ///</summary>
        ///<param name="entryA">First entry in the pair.</param>
        ///<param name="entryB">Second entry in the pair.</param>
        public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        {
            mesh = (MobileMeshCollidable)entryB;

            base.Initialize(entryA, entryB);
        }






        ///<summary>
        /// Cleans up the pair handler.
        ///</summary>
        public override void CleanUp()
        {

            base.CleanUp();
            mesh = null;


        }




        protected override void UpdateContainedPairs(Fixed64 dt)
        {
            var overlappedElements = CommonResources.GetIntList();
            BoundingBox localBoundingBox;
            AffineTransform meshTransform;
            AffineTransform.CreateFromRigidTransform(ref mesh.worldTransform, out meshTransform);

            FixedV3 sweep;
            FixedV3.Subtract(ref mobileMesh.entity.linearVelocity, ref mesh.entity.linearVelocity, out sweep);
            FixedV3.Multiply(ref sweep, dt, out sweep);
            mobileMesh.Shape.GetSweptLocalBoundingBox(ref mobileMesh.worldTransform, ref meshTransform, ref sweep, out localBoundingBox);
            mesh.Shape.TriangleMesh.Tree.GetOverlaps(localBoundingBox, overlappedElements);
            for (int i = 0; i < overlappedElements.Count; i++)
            {
                TryToAdd(overlappedElements.Elements[i]);
            }

            CommonResources.GiveBack(overlappedElements);

        }


    }
}

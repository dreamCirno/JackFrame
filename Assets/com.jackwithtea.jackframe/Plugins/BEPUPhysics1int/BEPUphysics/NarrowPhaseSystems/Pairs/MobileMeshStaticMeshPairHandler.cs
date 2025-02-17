﻿using System;
using BEPUPhysics1int.BroadPhaseEntries;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;
using BEPUPhysics1int.ResourceManagement;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a mobile mesh-static mesh collision pair.
    ///</summary>
    public class MobileMeshStaticMeshPairHandler : MobileMeshMeshPairHandler
    {


        StaticMesh mesh;

        public override Collidable CollidableB
        {
            get { return mesh; }
        }
        public override Entity EntityB
        {
            get { return null; }
        }
        protected override Materials.Material MaterialB
        {
            get { return mesh.material; }
        }

        protected override TriangleCollidable GetOpposingCollidable(int index)
        {
            //Construct a TriangleCollidable from the static mesh.
            var toReturn = PhysicsResources.GetTriangleCollidable();
            var shape = toReturn.Shape;
            mesh.Mesh.Data.GetTriangle(index, out shape.vA, out shape.vB, out shape.vC);
            FixedV3 center;
            FixedV3.Add(ref shape.vA, ref shape.vB, out center);
            FixedV3.Add(ref center, ref shape.vC, out center);
            FixedV3.Multiply(ref center, F64.OneThird, out center);
            FixedV3.Subtract(ref shape.vA, ref center, out shape.vA);
            FixedV3.Subtract(ref shape.vB, ref center, out shape.vB);
            FixedV3.Subtract(ref shape.vC, ref center, out shape.vC);
            //The bounding box doesn't update by itself.
            toReturn.worldTransform.Position = center;
            toReturn.worldTransform.Orientation = FixedQuaternion.Identity;
            toReturn.UpdateBoundingBoxInternal(F64.C0);
            shape.sidedness = mesh.sidedness;
            shape.collisionMargin = mobileMesh.Shape.MeshCollisionMargin;
            return toReturn;
        }

        protected override void ConfigureCollidable(TriangleEntry entry, Fixed64 dt)
        {

        }

        ///<summary>
        /// Initializes the pair handler.
        ///</summary>
        ///<param name="entryA">First entry in the pair.</param>
        ///<param name="entryB">Second entry in the pair.</param>
        public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        {
            mesh = entryA as StaticMesh;
            if (mesh == null)
            {
                mesh = entryB as StaticMesh;
                if (mesh == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
                }
            }


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
            mesh.Mesh.Tree.GetOverlaps(mobileMesh.boundingBox, overlappedElements);
            for (int i = 0; i < overlappedElements.Count; i++)
            {
                TryToAdd(overlappedElements.Elements[i]);
            }

            CommonResources.GiveBack(overlappedElements);

        }


    }
}

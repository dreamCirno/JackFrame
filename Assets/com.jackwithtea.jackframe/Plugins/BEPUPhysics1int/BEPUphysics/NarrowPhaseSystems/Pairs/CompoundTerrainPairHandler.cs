﻿using System;
using BEPUPhysics1int.BroadPhaseEntries;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a compound-terrain collision pair.
    ///</summary>
    public class CompoundTerrainPairHandler : CompoundGroupPairHandler
    {

        BEPUTerrain terrain;

        public override Collidable CollidableB
        {
            get { return terrain; }
        }
        public override Entity EntityB
        {
            get { return null; }
        }

        ///<summary>
        /// Initializes the pair handler.
        ///</summary>
        ///<param name="entryA">First entry in the pair.</param>
        ///<param name="entryB">Second entry in the pair.</param>
        public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        {
            terrain = entryA as BEPUTerrain;
            if (terrain == null)
            {

                terrain = entryB as BEPUTerrain;
                if (terrain == null)
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
            terrain = null;


        }





        protected override void UpdateContainedPairs()
        {           
            //Could go other way; get triangles in mesh that overlap the compound.
            //Could be faster sometimes depending on the way it's set up.
            var overlappedElements = PhysicsResources.GetCompoundChildList();
            compoundInfo.hierarchy.Tree.GetOverlaps(terrain.boundingBox, overlappedElements);
            for (int i = 0; i < overlappedElements.Count; i++)
            {
                TryToAdd(overlappedElements.Elements[i].CollisionInformation, terrain, overlappedElements.Elements[i].Material, terrain.Material);

            }

            PhysicsResources.GiveBack(overlappedElements);
        }



    }
}

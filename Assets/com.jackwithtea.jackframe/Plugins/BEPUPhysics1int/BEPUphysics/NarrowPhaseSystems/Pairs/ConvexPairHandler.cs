﻿using System;
using BEPUPhysics1int.BroadPhaseEntries;
using BEPUPhysics1int.BroadPhaseSystems;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;
using BEPUPhysics1int.CollisionTests;
using BEPUPhysics1int.CollisionTests.CollisionAlgorithms.GJK;
using BEPUPhysics1int.CollisionTests.Manifolds;
using BEPUPhysics1int.Constraints.Collision;
using BEPUPhysics1int.PositionUpdating;
using BEPUPhysics1int.Settings;
 
using BEPUPhysics1int.CollisionShapes.ConvexShapes;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Pair handler that manages a pair of two convex shapes.
    ///</summary>
    public abstract class ConvexPairHandler : StandardPairHandler
    {

        public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        {
            UpdateMaterialProperties();

            base.Initialize(entryA, entryB);
        }



        ///<summary>
        /// Updates the time of impact for the pair.
        ///</summary>
        ///<param name="requester">Collidable requesting the update.</param>
        ///<param name="dt">Timestep duration.</param>
        public override void UpdateTimeOfImpact(Collidable requester, Fixed64 dt)
        {
            var collidableA = CollidableA as ConvexCollidable;
            var collidableB = CollidableB as ConvexCollidable;
            var modeA = collidableA.entity == null ? PositionUpdateMode.Discrete : collidableA.entity.PositionUpdateMode;
            var modeB = collidableB.entity == null ? PositionUpdateMode.Discrete : collidableB.entity.PositionUpdateMode;

            var overlap = BroadPhaseOverlap;
            if (
                    (overlap.entryA.IsActive || overlap.entryB.IsActive) && //At least one has to be active.
                    (
                        (
                            modeA == PositionUpdateMode.Continuous &&   //If both are continuous, only do the process for A.
                            modeB == PositionUpdateMode.Continuous &&
                            overlap.entryA == requester
                        ) ||
                        (
                            modeA == PositionUpdateMode.Continuous ^   //If only one is continuous, then we must do it.
                            modeB == PositionUpdateMode.Continuous
                        )

                    )
                )
            {
                //Only perform the test if the minimum radii are small enough relative to the size of the velocity.
                //Discrete objects have already had their linear motion integrated, so don't use their velocity.
                FixedV3 velocity;
                if (modeA == PositionUpdateMode.Discrete)
                {
                    //CollidableA is static for the purposes of this continuous test.
                    velocity = collidableB.entity.linearVelocity;
                }
                else if (modeB == PositionUpdateMode.Discrete)
                {
                    //CollidableB is static for the purposes of this continuous test.
                    FixedV3.Negate(ref collidableA.entity.linearVelocity, out velocity);
                }
                else
                {
                    //Both objects are moving.
                    FixedV3.Subtract(ref collidableB.entity.linearVelocity, ref collidableA.entity.linearVelocity, out velocity);
                }
                FixedV3.Multiply(ref velocity, dt, out velocity);
                Fixed64 velocitySquared = velocity.LengthSquared();

                var minimumRadiusA = collidableA.Shape.MinimumRadius * MotionSettings.CoreShapeScaling;
                timeOfImpact = F64.C1;
                if (minimumRadiusA * minimumRadiusA < velocitySquared)
                {
                    //Spherecast A against B.
                    RayHit rayHit;
                    if (GJKToolbox.CCDSphereCast(new BEPURay(collidableA.worldTransform.Position, -velocity), minimumRadiusA, collidableB.Shape, ref collidableB.worldTransform, timeOfImpact, out rayHit))
                        timeOfImpact = rayHit.T;
                }

                var minimumRadiusB = collidableB.Shape.MinimumRadius * MotionSettings.CoreShapeScaling;
                if (minimumRadiusB * minimumRadiusB < velocitySquared)
                {
                    //Spherecast B against A.
                    RayHit rayHit;
                    if (GJKToolbox.CCDSphereCast(new BEPURay(collidableB.worldTransform.Position, velocity), minimumRadiusB, collidableA.Shape, ref collidableA.worldTransform, timeOfImpact, out rayHit))
                        timeOfImpact = rayHit.T;
                }

                //If it's intersecting, throw our hands into the air and give up.
                //This is generally a perfectly acceptable thing to do, since it's either sitting
                //inside another object (no ccd makes sense) or we're still in an intersecting case
                //from a previous frame where CCD took place and a contact should have been created
                //to deal with interpenetrating velocity.  Sometimes that contact isn't sufficient,
                //but it's good enough.
                if (timeOfImpact == F64.C0)
                    timeOfImpact = F64.C1;
            }

        }


    }

}

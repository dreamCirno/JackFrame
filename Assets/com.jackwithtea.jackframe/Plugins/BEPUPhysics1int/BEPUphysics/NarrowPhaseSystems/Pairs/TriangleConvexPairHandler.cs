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
    /// Handles a triangle-convex collision pair.
    ///</summary>
    public class TriangleConvexPairHandler : ConvexConstraintPairHandler
    {
        ConvexCollidable<TriangleShape> triangle;
        ConvexCollidable convex;

        TriangleConvexContactManifold contactManifold = new TriangleConvexContactManifold();

        public override Collidable CollidableA
        {
            get { return convex; }
        }
        public override Collidable CollidableB
        {
            get { return triangle; }
        }

        public override Entity EntityA
        {
            get { return convex.entity; }
        }
        public override Entity EntityB
        {
            get { return triangle.entity; }
        }
        /// <summary>
        /// Gets the contact manifold used by the pair handler.
        /// </summary>
        public override ContactManifold ContactManifold
        {
            get { return contactManifold; }
        }



        ///<summary>
        /// Initializes the pair handler.
        ///</summary>
        ///<param name="entryA">First entry in the pair.</param>
        ///<param name="entryB">Second entry in the pair.</param>
        public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        {

            triangle = entryA as ConvexCollidable<TriangleShape>;
            convex = entryB as ConvexCollidable;

            if (triangle == null || convex == null)
            {
                triangle = entryB as ConvexCollidable<TriangleShape>;
                convex = entryA as ConvexCollidable;

                if (triangle == null || convex == null)
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
            }

            //Contact normal goes from A to B.
            broadPhaseOverlap.entryA = convex;
            broadPhaseOverlap.entryB = triangle;

            base.Initialize(entryA, entryB);

        }

        ///<summary>
        /// Cleans up the pair handler.
        ///</summary>
        public override void CleanUp()
        {
            base.CleanUp();

            triangle = null;
            convex = null;

        }




        ///<summary>
        /// Updates the time of impact for the pair.
        ///</summary>
        ///<param name="requester">Collidable requesting the update.</param>
        ///<param name="dt">Timestep duration.</param>
        public override void UpdateTimeOfImpact(Collidable requester, Fixed64 dt)
        {
            var overlap = BroadPhaseOverlap;
            var triangleMode = triangle.entity == null ? PositionUpdateMode.Discrete : triangle.entity.PositionUpdateMode;
            var convexMode = convex.entity == null ? PositionUpdateMode.Discrete : convex.entity.PositionUpdateMode;
            if (
                    (overlap.entryA.IsActive || overlap.entryB.IsActive) && //At least one has to be active.
                    (
                        (
                            convexMode == PositionUpdateMode.Continuous &&   //If both are continuous, only do the process for A.
                            triangleMode == PositionUpdateMode.Continuous &&
                            overlap.entryA == requester
                        ) ||
                        (
                            convexMode == PositionUpdateMode.Continuous ^   //If only one is continuous, then we must do it.
                            triangleMode == PositionUpdateMode.Continuous
                        )
                    )
                )
            {


                //Only perform the test if the minimum radii are small enough relative to the size of the velocity.
                FixedV3 velocity;
                if (convexMode == PositionUpdateMode.Discrete)
                {
                    //Triangle is static for the purposes of this continuous test.
                    velocity = triangle.entity.linearVelocity;
                }
                else if (triangleMode == PositionUpdateMode.Discrete)
                {
                    //Convex is static for the purposes of this continuous test.
                    FixedV3.Negate(ref convex.entity.linearVelocity, out velocity);
                }
                else
                {
                    //Both objects are moving.
                    FixedV3.Subtract(ref triangle.entity.linearVelocity, ref convex.entity.linearVelocity, out velocity);
                }
                FixedV3.Multiply(ref velocity, dt, out velocity);
                Fixed64 velocitySquared = velocity.LengthSquared();

                var minimumRadiusA = convex.Shape.MinimumRadius * MotionSettings.CoreShapeScaling;
                timeOfImpact = F64.C1;
                if (minimumRadiusA * minimumRadiusA < velocitySquared)
                {
                    //Spherecast A against B.
                    RayHit rayHit;
                    if (GJKToolbox.CCDSphereCast(new BEPURay(convex.worldTransform.Position, -velocity), minimumRadiusA, triangle.Shape, ref triangle.worldTransform, timeOfImpact, out rayHit))
                    {
                        if (triangle.Shape.sidedness != TriangleSidedness.DoubleSided)
                        {                
                            //Only perform sweep if the object is in danger of hitting the object.
                            //Triangles can be one sided, so check the impact normal against the triangle normal.
                            FixedV3 AB, AC;
                            FixedV3.Subtract(ref triangle.Shape.vB, ref triangle.Shape.vA, out AB);
                            FixedV3.Subtract(ref triangle.Shape.vC, ref triangle.Shape.vA, out AC);
                            FixedV3 normal;
                            FixedV3.Cross(ref AB, ref AC, out normal);

                            Fixed64 dot;
                            FixedV3.Dot(ref rayHit.Normal, ref normal, out dot);
                            if (triangle.Shape.sidedness == TriangleSidedness.Counterclockwise && dot < F64.C0 ||
                                triangle.Shape.sidedness == TriangleSidedness.Clockwise && dot > F64.C0)
                            {
                                timeOfImpact = rayHit.T;
                            }
                        }
                        else
                        {
                            timeOfImpact = rayHit.T;
                        }
                    }
                }

                //TECHNICALLY, the triangle should be casted too.  But, given the way triangles are usually used and their tiny minimum radius, ignoring it is usually just fine.
                //var minimumRadiusB = triangle.minimumRadius * MotionSettings.CoreShapeScaling;
                //if (minimumRadiusB * minimumRadiusB < velocitySquared)
                //{
                //    //Spherecast B against A.
                //    RayHit rayHit;
                //    if (GJKToolbox.SphereCast(new Ray(triangle.entity.position, velocity), minimumRadiusB, convex.Shape, ref convex.worldTransform, 1, out rayHit) &&
                //        rayHit.T < timeOfImpact)
                //    {
                //        if (triangle.Shape.sidedness != TriangleSidedness.DoubleSided)
                //        {
                //            Fix64 dot;
                //            Vector3.Dot(ref rayHit.Normal, ref normal, out dot);
                //            if (dot > 0)
                //            {
                //                timeOfImpact = rayHit.T;
                //            }
                //        }
                //        else
                //        {
                //            timeOfImpact = rayHit.T;
                //        }
                //    }
                //}

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

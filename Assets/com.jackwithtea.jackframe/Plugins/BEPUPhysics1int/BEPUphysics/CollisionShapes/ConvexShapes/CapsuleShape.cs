﻿using System;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;

using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.CollisionShapes.ConvexShapes
{
    ///<summary>
    /// Sphere-expanded line segment.  Another way of looking at it is a cylinder with half-spheres on each end.
    ///</summary>
    public class CapsuleShape : ConvexShape
    {
        Fixed64 halfLength;
        ///<summary>
        /// Gets or sets the length of the capsule's inner line segment.
        ///</summary>
        public Fixed64 Length
        {
            get
            {
                return halfLength * F64.C2;
            }
            set
            {
                halfLength = value * F64.C0p5;
                OnShapeChanged();
            }
        }

        //This is a convenience method.  People expect to see a 'radius' of some kind.
        ///<summary>
        /// Gets or sets the radius of the capsule.
        ///</summary>
        public Fixed64 Radius { get { return collisionMargin; } set { CollisionMargin = value; } }


        ///<summary>
        /// Constructs a new capsule shape.
        ///</summary>
        ///<param name="length">Length of the capsule's inner line segment.</param>
        ///<param name="radius">Radius to expand the line segment width.</param>
        public CapsuleShape(Fixed64 length, Fixed64 radius)
        {
            halfLength = length * F64.C0p5;

            UpdateConvexShapeInfo(ComputeDescription(length, radius));
        }

        ///<summary>
        /// Constructs a new capsule shape from cached information.
        ///</summary>
        ///<param name="length">Length of the capsule's inner line segment.</param>
        /// <param name="description">Cached information about the shape. Assumed to be correct; no extra processing or validation is performed.</param>
        public CapsuleShape(Fixed64 length, ConvexShapeDescription description)
        {
            halfLength = length * F64.C0p5;

            UpdateConvexShapeInfo(description);
        }



        protected override void OnShapeChanged()
        {
            UpdateConvexShapeInfo(ComputeDescription(halfLength * F64.C2, Radius));
            base.OnShapeChanged();
        }


        /// <summary>
        /// Computes a convex shape description for a CapsuleShape.
        /// </summary>
        ///<param name="length">Length of the capsule's inner line segment.</param>
        ///<param name="radius">Radius to expand the line segment width.</param>
        /// <returns>Description required to define a convex shape.</returns>
        public static ConvexShapeDescription ComputeDescription(Fixed64 length, Fixed64 radius)
        {
            ConvexShapeDescription description;
            description.EntityShapeVolume.Volume = MathHelper.Pi * radius * radius * length + F64.FourThirds * MathHelper.Pi * radius * radius * radius;

            description.EntityShapeVolume.VolumeDistribution = new BEPUMatrix3x3();
            Fixed64 effectiveLength = length + radius / F64.C2; //This is a cylindrical inertia tensor. Approximate.
            Fixed64 diagValue = F64.C0p0833333333 * effectiveLength * effectiveLength + F64.C0p25 * radius * radius;
            description.EntityShapeVolume.VolumeDistribution.M11 = diagValue;
            description.EntityShapeVolume.VolumeDistribution.M22 = F64.C0p5 * radius * radius;
            description.EntityShapeVolume.VolumeDistribution.M33 = diagValue;

            description.MaximumRadius = length * F64.C0p5 + radius;
            description.MinimumRadius = radius;

            description.CollisionMargin = radius;
            return description;
        }

        public override void GetBoundingBox(ref RigidTransform shapeTransform, out BoundingBox boundingBox)
        {
#if !WINDOWS
            boundingBox = new BoundingBox();
#endif
            FixedV3 upExtreme;
            FixedQuaternion.TransformY(halfLength, ref shapeTransform.Orientation, out upExtreme);

            if (upExtreme.X > F64.C0)
            {
                boundingBox.Max.X = upExtreme.X + collisionMargin;
                boundingBox.Min.X = -upExtreme.X - collisionMargin;
            }
            else
            {
                boundingBox.Max.X = -upExtreme.X + collisionMargin;
                boundingBox.Min.X = upExtreme.X - collisionMargin;
            }

            if (upExtreme.Y > F64.C0)
            {
                boundingBox.Max.Y = upExtreme.Y + collisionMargin;
                boundingBox.Min.Y = -upExtreme.Y - collisionMargin;
            }
            else
            {
                boundingBox.Max.Y = -upExtreme.Y + collisionMargin;
                boundingBox.Min.Y = upExtreme.Y - collisionMargin;
            }

            if (upExtreme.Z > F64.C0)
            {
                boundingBox.Max.Z = upExtreme.Z + collisionMargin;
                boundingBox.Min.Z = -upExtreme.Z - collisionMargin;
            }
            else
            {
                boundingBox.Max.Z = -upExtreme.Z + collisionMargin;
                boundingBox.Min.Z = upExtreme.Z - collisionMargin;
            }

            FixedV3.Add(ref shapeTransform.Position, ref boundingBox.Min, out boundingBox.Min);
            FixedV3.Add(ref shapeTransform.Position, ref boundingBox.Max, out boundingBox.Max);
        }


        ///<summary>
        /// Gets the extreme point of the shape in local space in a given direction.
        ///</summary>
        ///<param name="direction">Direction to find the extreme point in.</param>
        ///<param name="extremePoint">Extreme point on the shape.</param>
        public override void GetLocalExtremePointWithoutMargin(ref FixedV3 direction, out FixedV3 extremePoint)
        {
            if (direction.Y > F64.C0)
                extremePoint = new FixedV3(F64.C0, halfLength, F64.C0);
            else if (direction.Y < F64.C0)
                extremePoint = new FixedV3(F64.C0, -halfLength, F64.C0);
            else
                extremePoint = Toolbox.ZeroVector;
        }




        /// <summary>
        /// Retrieves an instance of an EntityCollidable that uses this EntityShape.  Mainly used by compound bodies.
        /// </summary>
        /// <returns>EntityCollidable that uses this shape.</returns>
        public override EntityCollidable GetCollidableInstance()
        {
            return new ConvexCollidable<CapsuleShape>(this);
        }

        /// <summary>
        /// Gets the intersection between the convex shape and the ray.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="transform">Transform of the convex shape.</param>
        /// <param name="maximumLength">Maximum distance to travel in units of the ray direction's length.</param>
        /// <param name="hit">Ray hit data, if any.</param>
        /// <returns>Whether or not the ray hit the target.</returns>
        public override bool RayTest(ref BEPURay ray, ref RigidTransform transform, Fixed64 maximumLength, out RayHit hit)
        {
            //Put the ray into local space.
            FixedQuaternion conjugate;
            FixedQuaternion.Conjugate(ref transform.Orientation, out conjugate);
            BEPURay localRay;
            FixedV3.Subtract(ref ray.Position, ref transform.Position, out localRay.Position);
            FixedQuaternion.Transform(ref localRay.Position, ref conjugate, out localRay.Position);
            FixedQuaternion.Transform(ref ray.Direction, ref conjugate, out localRay.Direction);

            //Check for containment in the cylindrical portion of the capsule.
            if (localRay.Position.Y >= -halfLength && localRay.Position.Y <= halfLength && localRay.Position.X * localRay.Position.X + localRay.Position.Z * localRay.Position.Z <= collisionMargin * collisionMargin)
            {
                //It's inside!
                hit.T = F64.C0;
                hit.Location = localRay.Position;
                hit.Normal = new FixedV3(hit.Location.X, F64.C0, hit.Location.Z);
                Fixed64 normalLengthSquared = hit.Normal.LengthSquared();
                if (normalLengthSquared > F64.C1em9)
                    FixedV3.Divide(ref hit.Normal, Fixed64.Sqrt(normalLengthSquared), out hit.Normal);
                else
                    hit.Normal = new FixedV3();
                //Pull the hit into world space.
                FixedQuaternion.Transform(ref hit.Normal, ref transform.Orientation, out hit.Normal);
                RigidTransform.Transform(ref hit.Location, ref transform, out hit.Location);
                return true;
            }

            //Project the ray direction onto the plane where the cylinder is a circle.
            //The projected ray is then tested against the circle to compute the time of impact.
            //That time of impact is used to compute the 3d hit location.
            FixedV2 planeDirection = new FixedV2(localRay.Direction.X, localRay.Direction.Z);
            Fixed64 planeDirectionLengthSquared = planeDirection.LengthSquared();

            if (planeDirectionLengthSquared < Toolbox.Epsilon)
            {
                //The ray is nearly parallel with the axis.
                //Skip the cylinder-sides test.  We're either inside the cylinder and won't hit the sides, or we're outside
                //and won't hit the sides.  
                if (localRay.Position.Y > halfLength)
                    goto upperSphereTest;
                if (localRay.Position.Y < -halfLength)
                    goto lowerSphereTest;


                hit = new RayHit();
                return false;

            }
            FixedV2 planeOrigin = new FixedV2(localRay.Position.X, localRay.Position.Z);
            Fixed64 dot;
            FixedV2.Dot(ref planeDirection, ref planeOrigin, out dot);
            Fixed64 closestToCenterT = -dot / planeDirectionLengthSquared;

            FixedV2 closestPoint;
            FixedV2.Multiply(ref planeDirection, closestToCenterT, out closestPoint);
            FixedV2.Add(ref planeOrigin, ref closestPoint, out closestPoint);
            //How close does the ray come to the circle?
            Fixed64 squaredDistance = closestPoint.LengthSquared();
            if (squaredDistance > collisionMargin * collisionMargin)
            {
                //It's too far!  The ray cannot possibly hit the capsule.
                hit = new RayHit();
                return false;
            }



            //With the squared distance, compute the distance backward along the ray from the closest point on the ray to the axis.
            Fixed64 backwardsDistance = collisionMargin * Fixed64.Sqrt(F64.C1 - squaredDistance / (collisionMargin * collisionMargin));
            Fixed64 tOffset = backwardsDistance / Fixed64.Sqrt(planeDirectionLengthSquared);

            hit.T = closestToCenterT - tOffset;

            //Compute the impact point on the infinite cylinder in 3d local space.
            FixedV3.Multiply(ref localRay.Direction, hit.T, out hit.Location);
            FixedV3.Add(ref hit.Location, ref localRay.Position, out hit.Location);

            //Is it intersecting the cylindrical portion of the capsule?
            if (hit.Location.Y <= halfLength && hit.Location.Y >= -halfLength && hit.T < maximumLength)
            {
                //Yup!
                hit.Normal = new FixedV3(hit.Location.X, F64.C0, hit.Location.Z);
                Fixed64 normalLengthSquared = hit.Normal.LengthSquared();
                if (normalLengthSquared > F64.C1em9)
                    FixedV3.Divide(ref hit.Normal, Fixed64.Sqrt(normalLengthSquared), out hit.Normal);
                else
                    hit.Normal = new FixedV3();
                //Pull the hit into world space.
                FixedQuaternion.Transform(ref hit.Normal, ref transform.Orientation, out hit.Normal);
                RigidTransform.Transform(ref hit.Location, ref transform, out hit.Location);
                return true;
            }

            if (hit.Location.Y < halfLength)
                goto lowerSphereTest;
        upperSphereTest:
            //Nope! It may be intersecting the ends of the capsule though.
            //We're above the capsule, so cast a ray against the upper sphere.
            //We don't have to worry about it hitting the bottom of the sphere since it would have hit the cylinder portion first.
            var spherePosition = new FixedV3(F64.C0, halfLength, F64.C0);
            if (Toolbox.RayCastSphere(ref localRay, ref spherePosition, collisionMargin, maximumLength, out hit))
            {
                //Pull the hit into world space.
                FixedQuaternion.Transform(ref hit.Normal, ref transform.Orientation, out hit.Normal);
                RigidTransform.Transform(ref hit.Location, ref transform, out hit.Location);
                return true;
            }
            //No intersection! We can't be hitting the other sphere, so it's over!
            hit = new RayHit();
            return false;

        lowerSphereTest:
            //Okay, what about the bottom sphere?
            //We're above the capsule, so cast a ray against the upper sphere.
            //We don't have to worry about it hitting the bottom of the sphere since it would have hit the cylinder portion first.
            spherePosition = new FixedV3(F64.C0, -halfLength, F64.C0);
            if (Toolbox.RayCastSphere(ref localRay, ref spherePosition, collisionMargin, maximumLength, out hit))
            {
                //Pull the hit into world space.
                FixedQuaternion.Transform(ref hit.Normal, ref transform.Orientation, out hit.Normal);
                RigidTransform.Transform(ref hit.Location, ref transform, out hit.Location);
                return true;
            }
            //No intersection! We can't be hitting the other sphere, so it's over!
            hit = new RayHit();
            return false;

        }

    }
}

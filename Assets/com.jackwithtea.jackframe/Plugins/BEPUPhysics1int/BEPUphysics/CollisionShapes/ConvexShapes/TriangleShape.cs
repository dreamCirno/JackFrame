using System;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;

using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.CollisionShapes.ConvexShapes
{

    ///<summary>
    /// Triangle collision shape.
    ///</summary>
    public class TriangleShape : ConvexShape
    {
        internal FixedV3 vA, vB, vC;

        ///<summary>
        /// Gets or sets the first vertex of the triangle shape.
        ///</summary>
        public FixedV3 VertexA
        {
            get
            {
                return vA;
            }
            set
            {
                vA = value;
                OnShapeChanged();
            }
        }

        ///<summary>
        /// Gets or sets the second vertex of the triangle shape.
        ///</summary>
        public FixedV3 VertexB
        {
            get
            {
                return vB;
            }
            set
            {
                vB = value;
                OnShapeChanged();
            }
        }

        ///<summary>
        /// Gets or sets the third vertex of the triangle shape.
        ///</summary>
        public FixedV3 VertexC
        {
            get
            {
                return vC;
            }
            set
            {
                vC = value;
                OnShapeChanged();
            }
        }

        internal TriangleSidedness sidedness;
        ///<summary>
        /// Gets or sets the sidedness of the triangle.
        ///</summary>
        public TriangleSidedness Sidedness
        {
            get { return sidedness; }
            set
            {
                sidedness = value;
                OnShapeChanged();
            }
        }

        ///<summary>
        /// Constructs a triangle shape without initializing it.
        /// This is useful for systems that re-use a triangle shape repeatedly and do not care about its properties.
        ///</summary>
        public TriangleShape()
        {
            //Triangles are often used in special situations where the vertex locations are changed directly.  This constructor assists with that.
        }

        ///<summary>
        /// Constructs a triangle shape.
        /// The vertices will be recentered.  If the center is needed, use the other constructor.
        ///</summary>
        ///<param name="vA">First vertex in the triangle.</param>
        ///<param name="vB">Second vertex in the triangle.</param>
        ///<param name="vC">Third vertex in the triangle.</param>
        public TriangleShape(FixedV3 vA, FixedV3 vB, FixedV3 vC)
        {
            //Recenter.  Convexes should contain the origin.
            FixedV3 center = (vA + vB + vC) / F64.C3;
            this.vA = vA - center;
            this.vB = vB - center;
            this.vC = vC - center;
            UpdateConvexShapeInfo(ComputeDescription(this.vA, this.vB, this.vC, collisionMargin));
        }

        ///<summary>
        /// Constructs a triangle shape.
        /// The vertices will be recentered.
        ///</summary>
        ///<param name="vA">First vertex in the triangle.</param>
        ///<param name="vB">Second vertex in the triangle.</param>
        ///<param name="vC">Third vertex in the triangle.</param>
        ///<param name="center">Computed center of the triangle.</param>
        public TriangleShape(FixedV3 vA, FixedV3 vB, FixedV3 vC, out FixedV3 center)
        {
            //Recenter.  Convexes should contain the origin.
            center = (vA + vB + vC) / F64.C3;
            this.vA = vA - center;
            this.vB = vB - center;
            this.vC = vC - center;
            UpdateConvexShapeInfo(ComputeDescription(this.vA, this.vB, this.vC, collisionMargin));
        }

        ///<summary>
        /// Constructs a triangle shape from cached data.
        ///</summary>
        ///<param name="vA">First vertex in the triangle.</param>
        ///<param name="vB">Second vertex in the triangle.</param>
        ///<param name="vC">Third vertex in the triangle.</param>
        /// <param name="description">Cached information about the shape. Assumed to be correct; no extra processing or validation is performed.</param>
        public TriangleShape(FixedV3 vA, FixedV3 vB, FixedV3 vC, ConvexShapeDescription description)
        {
            //Recenter.  Convexes should contain the origin.
            var center = (vA + vB + vC) / F64.C3;
            this.vA = vA - center;
            this.vB = vB - center;
            this.vC = vC - center;
            UpdateConvexShapeInfo(description);
        }




        /// <summary>
        /// Computes a convex shape description for a TransformableShape.
        /// </summary>
        ///<param name="vA">First local vertex in the triangle.</param>
        ///<param name="vB">Second local vertex in the triangle.</param>
        ///<param name="vC">Third local vertex in the triangle.</param>
        ///<param name="collisionMargin">Collision margin of the shape.</param>
        /// <returns>Description required to define a convex shape.</returns>
        public static ConvexShapeDescription ComputeDescription(FixedV3 vA, FixedV3 vB, FixedV3 vC, Fixed64 collisionMargin)
        {
            ConvexShapeDescription description;
            // A triangle by itself technically has no volume, but shapes try to include the collision margin in the volume when feasible (e.g. BoxShape).
            //Plus, it's convenient to have a nonzero volume for buoyancy.
            var doubleArea = FixedV3.Cross(vB - vA, vC - vA).Length();
            description.EntityShapeVolume.Volume = doubleArea * collisionMargin;

            //Compute the inertia tensor.
            var v = new BEPUMatrix3x3(
                vA.X, vA.Y, vA.Z,
                vB.X, vB.Y, vB.Z,
                vC.X, vC.Y, vC.Z);
            var s = new BEPUMatrix3x3(
				F64.C2, F64.C1, F64.C1,
				F64.C1, F64.C2, F64.C1,
				F64.C1, F64.C1, F64.C2);

            BEPUMatrix3x3.MultiplyTransposed(ref v, ref s, out description.EntityShapeVolume.VolumeDistribution);
            BEPUMatrix3x3.Multiply(ref description.EntityShapeVolume.VolumeDistribution, ref v, out description.EntityShapeVolume.VolumeDistribution);
            var scaling = doubleArea / F64.C24;
            BEPUMatrix3x3.Multiply(ref description.EntityShapeVolume.VolumeDistribution, -scaling, out description.EntityShapeVolume.VolumeDistribution);

            //The square-of-sum term is ignored since the parameters should already be localized (and so would sum to zero).
            var sums = scaling * (vA.LengthSquared() + vB.LengthSquared() + vC.LengthSquared());
            description.EntityShapeVolume.VolumeDistribution.M11 += sums;
            description.EntityShapeVolume.VolumeDistribution.M22 += sums;
            description.EntityShapeVolume.VolumeDistribution.M33 += sums;

            description.MinimumRadius = collisionMargin;
            description.MaximumRadius = collisionMargin + MathHelper.Max(vA.Length(), MathHelper.Max(vB.Length(), vC.Length()));
            description.CollisionMargin = collisionMargin;
            return description;
        }

        /// <summary>
        /// Gets the bounding box of the shape given a transform.
        /// </summary>
        /// <param name="shapeTransform">Transform to use.</param>
        /// <param name="boundingBox">Bounding box of the transformed shape.</param>
        public override void GetBoundingBox(ref RigidTransform shapeTransform, out BoundingBox boundingBox)
        {
            FixedV3 a, b, c;

            BEPUMatrix3x3 o;
            BEPUMatrix3x3.CreateFromQuaternion(ref shapeTransform.Orientation, out o);
            BEPUMatrix3x3.Transform(ref vA, ref o, out a);
            BEPUMatrix3x3.Transform(ref vB, ref o, out b);
            BEPUMatrix3x3.Transform(ref vC, ref o, out c);

            FixedV3.Min(ref a, ref b, out boundingBox.Min);
            FixedV3.Min(ref c, ref boundingBox.Min, out boundingBox.Min);

            FixedV3.Max(ref a, ref b, out boundingBox.Max);
            FixedV3.Max(ref c, ref boundingBox.Max, out boundingBox.Max);

            boundingBox.Min.X += shapeTransform.Position.X - collisionMargin;
            boundingBox.Min.Y += shapeTransform.Position.Y - collisionMargin;
            boundingBox.Min.Z += shapeTransform.Position.Z - collisionMargin;
            boundingBox.Max.X += shapeTransform.Position.X + collisionMargin;
            boundingBox.Max.Y += shapeTransform.Position.Y + collisionMargin;
            boundingBox.Max.Z += shapeTransform.Position.Z + collisionMargin;
        }


        ///<summary>
        /// Gets the extreme point of the shape in local space in a given direction.
        ///</summary>
        ///<param name="direction">Direction to find the extreme point in.</param>
        ///<param name="extremePoint">Extreme point on the shape.</param>
        public override void GetLocalExtremePointWithoutMargin(ref FixedV3 direction, out FixedV3 extremePoint)
        {
            Fixed64 dotA, dotB, dotC;
            FixedV3.Dot(ref direction, ref vA, out dotA);
            FixedV3.Dot(ref direction, ref vB, out dotB);
            FixedV3.Dot(ref direction, ref vC, out dotC);
            if (dotA > dotB && dotA > dotC)
            {
                extremePoint = vA;
            }
            else if (dotB > dotC) //vA is not the most extreme point.
            {
                extremePoint = vB;
            }
            else
            {
                extremePoint = vC;
            }
        }

        /// <summary>
        /// Computes the volume distribution of the triangle.
        /// The volume distribution can be used to compute inertia tensors when
        /// paired with mass and other tuning factors.
        /// </summary>
        ///<param name="vA">First vertex in the triangle.</param>
        ///<param name="vB">Second vertex in the triangle.</param>
        ///<param name="vC">Third vertex in the triangle.</param>
        /// <returns>Volume distribution of the shape.</returns>
        public static BEPUMatrix3x3 ComputeVolumeDistribution(FixedV3 vA, FixedV3 vB, FixedV3 vC)
        {
            FixedV3 center = (vA + vB + vC) * F64.OneThird;

            //Calculate distribution of mass.

            Fixed64 massPerPoint = F64.OneThird;

            //Subtract the position from the distribution, moving into a 'body space' relative to itself.
            //        [ (j * j + z * z)  (-j * j)  (-j * z) ]
            //I = I + [ (-j * j)  (j * j + z * z)  (-j * z) ]
            //	      [ (-j * z)  (-j * z)  (j * j + j * j) ]

            Fixed64 i = vA.X - center.X;
            Fixed64 j = vA.Y - center.Y;
            Fixed64 k = vA.Z - center.Z;
            //localInertiaTensor += new Matrix(j * j + k * k, -j * j, -j * k, 0, -j * j, j * j + k * k, -j * k, 0, -j * k, -j * k, j * j + j * j, 0, 0, 0, 0, 0); //No mass per point.
            var volumeDistribution = new BEPUMatrix3x3(massPerPoint * (j * j + k * k), massPerPoint * (-i * j), massPerPoint * (-i * k),
                                                   massPerPoint * (-i * j), massPerPoint * (i * i + k * k), massPerPoint * (-j * k),
                                                   massPerPoint * (-i * k), massPerPoint * (-j * k), massPerPoint * (i * i + j * j));

            i = vB.X - center.X;
            j = vB.Y - center.Y;
            k = vB.Z - center.Z;
            var pointContribution = new BEPUMatrix3x3(massPerPoint * (j * j + k * k), massPerPoint * (-i * j), massPerPoint * (-i * k),
                                                  massPerPoint * (-i * j), massPerPoint * (i * i + k * k), massPerPoint * (-j * k),
                                                  massPerPoint * (-i * k), massPerPoint * (-j * k), massPerPoint * (i * i + j * j));
            BEPUMatrix3x3.Add(ref volumeDistribution, ref pointContribution, out volumeDistribution);

            i = vC.X - center.X;
            j = vC.Y - center.Y;
            k = vC.Z - center.Z;
            pointContribution = new BEPUMatrix3x3(massPerPoint * (j * j + k * k), massPerPoint * (-i * j), massPerPoint * (-i * k),
                                              massPerPoint * (-i * j), massPerPoint * (i * i + k * k), massPerPoint * (-j * k),
                                              massPerPoint * (-i * k), massPerPoint * (-j * k), massPerPoint * (i * i + j * j));
            BEPUMatrix3x3.Add(ref volumeDistribution, ref pointContribution, out volumeDistribution);
            return volumeDistribution;
        }

        ///<summary>
        /// Gets the normal of the triangle shape in its local space.
        ///</summary>
        ///<returns>The local normal.</returns>
        public FixedV3 GetLocalNormal()
        {
            FixedV3 normal;
            FixedV3 vAvB;
            FixedV3 vAvC;
            FixedV3.Subtract(ref vB, ref vA, out vAvB);
            FixedV3.Subtract(ref vC, ref vA, out vAvC);
            FixedV3.Cross(ref vAvB, ref vAvC, out normal);
            normal.Normalize();
            return normal;
        }

        /// <summary>
        /// Gets the normal of the triangle in world space.
        /// </summary>
        /// <param name="transform">World transform.</param>
        /// <returns>Normal of the triangle in world space.</returns>
        public FixedV3 GetNormal(RigidTransform transform)
        {
            FixedV3 normal = GetLocalNormal();
            FixedQuaternion.Transform(ref normal, ref transform.Orientation, out normal);
            return normal;
        }

        /// <summary>
        /// Gets the intersection between the triangle and the ray.
        /// </summary>
        /// <param name="ray">Ray to test against the triangle.</param>
        /// <param name="transform">Transform to apply to the triangle shape for the test.</param>
        /// <param name="maximumLength">Maximum distance to travel in units of the direction vector's length.</param>
        /// <param name="hit">Hit data of the ray cast, if any.</param>
        /// <returns>Whether or not the ray hit the target.</returns>
        public override bool RayTest(ref BEPURay ray, ref RigidTransform transform, Fixed64 maximumLength, out RayHit hit)
        {
            BEPUMatrix3x3 orientation;
            BEPUMatrix3x3.CreateFromQuaternion(ref transform.Orientation, out orientation);
            BEPURay localRay;
            FixedQuaternion conjugate;
            FixedQuaternion.Conjugate(ref transform.Orientation, out conjugate);
            FixedQuaternion.Transform(ref ray.Direction, ref conjugate, out localRay.Direction);
            FixedV3.Subtract(ref ray.Position, ref transform.Position, out localRay.Position);
            FixedQuaternion.Transform(ref localRay.Position, ref conjugate, out localRay.Position);

            bool toReturn = Toolbox.FindRayTriangleIntersection(ref localRay, maximumLength, sidedness, ref vA, ref vB, ref vC, out hit);
            //Move the hit back into world space.
            FixedV3.Multiply(ref ray.Direction, hit.T, out hit.Location);
            FixedV3.Add(ref ray.Position, ref hit.Location, out hit.Location);
            FixedQuaternion.Transform(ref hit.Normal, ref transform.Orientation, out hit.Normal);
            return toReturn;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return vA + ", " + vB + ", " + vC;
        }

        /// <summary>
        /// Retrieves an instance of an EntityCollidable that uses this EntityShape.  Mainly used by compound bodies.
        /// </summary>
        /// <returns>EntityCollidable that uses this shape.</returns>
        public override EntityCollidable GetCollidableInstance()
        {
            return new ConvexCollidable<TriangleShape>(this);
        }

    }

}

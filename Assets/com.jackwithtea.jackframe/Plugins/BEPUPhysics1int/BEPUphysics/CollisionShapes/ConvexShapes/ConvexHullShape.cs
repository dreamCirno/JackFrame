﻿using System;
using System.Collections.Generic;
using System.Linq;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;

using BEPUPhysics1int;
using BEPUPhysics1int.DataStructures;
using BEPUPhysics1int.ResourceManagement;
using FixMath.NET;

namespace BEPUPhysics1int.CollisionShapes.ConvexShapes
{
    ///<summary>
    /// Convex wrapping around a point set.
    ///</summary>
    public class ConvexHullShape : ConvexShape
    {
        ///<summary>
        /// Gets the point set of the convex hull.
        ///</summary>
        public ReadOnlyList<FixedV3> Vertices
        {
            get
            {
                return new ReadOnlyList<FixedV3>(vertices);
            }
        }
        FixedV3[] vertices;

        private readonly Fixed64 unexpandedMinimumRadius;
        private readonly Fixed64 unexpandedMaximumRadius;

        ///<summary>
        /// Constructs a new convex hull shape.
        /// The point set will be recentered on the local origin.
        /// If that offset is needed, use the other constructor which outputs the computed center.
        ///</summary>
        ///<param name="vertices">Point set to use to construct the convex hull.</param>
        ///<exception cref="ArgumentException">Thrown when the point set is empty.</exception>
        public ConvexHullShape(IList<FixedV3> vertices)
        {
            if (vertices.Count == 0)
                throw new ArgumentException("Vertices list used to create a ConvexHullShape cannot be empty.");

            var surfaceVertices = CommonResources.GetVectorList();
            var hullTriangleIndices = CommonResources.GetIntList();

            FixedV3 center;
            UpdateConvexShapeInfo(ComputeDescription(vertices, collisionMargin, out center, hullTriangleIndices, surfaceVertices));
            this.vertices = surfaceVertices.ToArray();


            CommonResources.GiveBack(hullTriangleIndices);
            CommonResources.GiveBack(surfaceVertices);

            unexpandedMaximumRadius = MaximumRadius - collisionMargin;
            unexpandedMinimumRadius = MinimumRadius - collisionMargin;
        }

        ///<summary>
        /// Constructs a new convex hull shape.
        /// The point set will be recentered on the local origin.
        ///</summary>
        ///<param name="vertices">Point set to use to construct the convex hull.</param>
        /// <param name="center">Computed center of the convex hull shape prior to recentering.</param>
        ///<exception cref="ArgumentException">Thrown when the point set is empty.</exception>
        public ConvexHullShape(IList<FixedV3> vertices, out FixedV3 center)
        {
            if (vertices.Count == 0)
                throw new ArgumentException("Vertices list used to create a ConvexHullShape cannot be empty.");

            var surfaceVertices = CommonResources.GetVectorList();
            var hullTriangleIndices = CommonResources.GetIntList();

            UpdateConvexShapeInfo(ComputeDescription(vertices, collisionMargin, out center, hullTriangleIndices, surfaceVertices));
            this.vertices = surfaceVertices.ToArray();

            CommonResources.GiveBack(hullTriangleIndices);
            CommonResources.GiveBack(surfaceVertices);

            unexpandedMaximumRadius = MaximumRadius - collisionMargin;
            unexpandedMinimumRadius = MinimumRadius - collisionMargin;

        }

        ///<summary>
        /// Constructs a new convex hull shape.
        /// The point set will be recentered on the local origin.
        ///</summary>
        ///<param name="vertices">Point set to use to construct the convex hull.</param>
        /// <param name="center">Computed center of the convex hull shape prior to recentering.</param>
        /// <param name="outputHullTriangleIndices">Triangle indices computed on the surface of the point set.</param>
        /// <param name="outputUniqueSurfaceVertices">Unique vertices on the surface of the convex hull.</param>
        ///<exception cref="ArgumentException">Thrown when the point set is empty.</exception>
        public ConvexHullShape(IList<FixedV3> vertices, out FixedV3 center, IList<int> outputHullTriangleIndices, IList<FixedV3> outputUniqueSurfaceVertices)
        {
            if (vertices.Count == 0)
                throw new ArgumentException("Vertices list used to create a ConvexHullShape cannot be empty.");

            UpdateConvexShapeInfo(ComputeDescription(vertices, collisionMargin, out center, outputHullTriangleIndices, outputUniqueSurfaceVertices));
            this.vertices = new FixedV3[outputUniqueSurfaceVertices.Count];
            outputUniqueSurfaceVertices.CopyTo(this.vertices, 0);

            unexpandedMaximumRadius = MaximumRadius - collisionMargin;
            unexpandedMinimumRadius = MinimumRadius - collisionMargin;

        }


        /// <summary>
        /// Creates a ConvexHullShape from cached information. Assumes all data provided is accurate- no pre-processing is performed.
        /// </summary>
        /// <param name="localSurfaceVertices">List of vertex positions on the surface of the convex hull shape, centered on the desired origin. These vertices are used as-is for the shape representation; no additional processing occurs.</param>
        /// <param name="description">Cached information about the shape. Assumed to be correct; no extra processing or validation is performed.</param>
        public ConvexHullShape(IList<FixedV3> localSurfaceVertices, ConvexShapeDescription description)
        {
            if (localSurfaceVertices.Count == 0)
                throw new ArgumentException("Vertices list used to create a ConvexHullShape cannot be empty.");

            unexpandedMaximumRadius = description.MaximumRadius - collisionMargin;
            unexpandedMinimumRadius = description.MinimumRadius - collisionMargin;
            vertices = new FixedV3[localSurfaceVertices.Count];
            localSurfaceVertices.CopyTo(vertices, 0);
            UpdateConvexShapeInfo(description);

        }

        protected override void OnShapeChanged()
        {
            //The convex hull shape's vertices are immutable.
            //The only way for this to occur is if the collision margin changed.
            //In that case, we only need to update the radius.

            //The (immutable) unexpanded radii are cached, so all that needs to be done is to add the new margin.
            UpdateConvexShapeInfo(new ConvexShapeDescription
            {
                EntityShapeVolume = new EntityShapeVolumeDescription { Volume = Volume, VolumeDistribution = VolumeDistribution },
                MinimumRadius = unexpandedMinimumRadius + collisionMargin,
                MaximumRadius = unexpandedMaximumRadius + collisionMargin,
                CollisionMargin = collisionMargin
            });
            base.OnShapeChanged();
        }

        /// <summary>
        /// Computes a convex shape description for a ConvexHullShape.
        /// </summary>
        /// <param name="vertices">Vertices describing the convex hull shape.</param>
        /// <param name="collisionMargin">Collision margin of the shape.</param>
        /// <param name="center">Computed center of the convex hull shape. Used as the origin of the outputUniqueSurfaceVertices.</param>
        /// <param name="outputHullTriangleIndices">Computed list of indices into the input point set composing the triangulated surface of the convex hull.
        /// Each group of 3 indices represents a triangle on the surface of the hull.</param>
        /// <param name="outputUniqueSurfaceVertices">Computed nonredundant list of vertices composing the outer shell of the input point set. Recentered on the local origin.</param>
        /// <returns>Description required to define a convex shape.</returns>
        public static ConvexShapeDescription ComputeDescription(IList<FixedV3> vertices, Fixed64 collisionMargin, out FixedV3 center, IList<int> outputHullTriangleIndices, IList<FixedV3> outputUniqueSurfaceVertices)
        {
            if (outputHullTriangleIndices.Count != 0 || outputUniqueSurfaceVertices.Count != 0)
                throw new ArgumentException("Output lists must start empty.");


            ConvexShapeDescription description;
            ConvexHullHelper.GetConvexHull(vertices, outputHullTriangleIndices, outputUniqueSurfaceVertices);

            InertiaHelper.ComputeShapeDistribution(vertices, outputHullTriangleIndices, out center, out description.EntityShapeVolume.Volume, out description.EntityShapeVolume.VolumeDistribution);
            //Recenter the surface vertices.
            for (int i = 0; i < outputUniqueSurfaceVertices.Count; ++i)
            {
                outputUniqueSurfaceVertices[i] -= center;
            }

            description.MinimumRadius = InertiaHelper.ComputeMinimumRadius(vertices, outputHullTriangleIndices, ref center) + collisionMargin;
            description.MaximumRadius = ComputeMaximumRadius(outputUniqueSurfaceVertices, collisionMargin);

            description.CollisionMargin = collisionMargin;
            return description;
        }


        /// <summary>
        /// Computes the minimum radius for the given convex hull data.
        /// </summary>
        /// <param name="localSurfaceVertices">Surface vertices of the convex hull.</param>
        /// <param name="collisionMargin">Collision margin of the shape.</param>
        /// <returns>Maximum radius of the convex hull.</returns>
        public static Fixed64 ComputeMaximumRadius(IList<FixedV3> localSurfaceVertices, Fixed64 collisionMargin)
        {
            Fixed64 longestLengthSquared = F64.C0;
            for (int i = 0; i < localSurfaceVertices.Count; ++i)
            {
                Fixed64 lengthCandidate = localSurfaceVertices[i].LengthSquared();
                if (lengthCandidate > longestLengthSquared)
                {
                    longestLengthSquared = lengthCandidate;
                }
            }
            return Fixed64.Sqrt(longestLengthSquared) + collisionMargin;
        }




        /// <summary>
        /// Gets the bounding box of the shape given a transform.
        /// </summary>
        /// <param name="shapeTransform">Transform to use.</param>
        /// <param name="boundingBox">Bounding box of the transformed shape.</param>
        public override void GetBoundingBox(ref RigidTransform shapeTransform, out BoundingBox boundingBox)
        {
#if !WINDOWS
            boundingBox = new BoundingBox();
#endif

            BEPUMatrix3x3 o;
            BEPUMatrix3x3.CreateFromQuaternion(ref shapeTransform.Orientation, out o);

            Fixed64 minX, maxX;
            Fixed64 minY, maxY;
            Fixed64 minZ, maxZ;
            var right = new FixedV3(o.M11, o.M21, o.M31);
            var up = new FixedV3(o.M12, o.M22, o.M32);
            var backward = new FixedV3(o.M13, o.M23, o.M33);
            FixedV3.Dot(ref vertices[0], ref right, out maxX);
            minX = maxX;
            FixedV3.Dot(ref vertices[0], ref up, out maxY);
            minY = maxY;
            FixedV3.Dot(ref vertices[0], ref backward, out maxZ);
            minZ = maxZ;
            int minXIndex = 0;
            int maxXIndex = 0;
            int minYIndex = 0;
            int maxYIndex = 0;
            int minZIndex = 0;
            int maxZIndex = 0;
            for (int i = 1; i < vertices.Length; ++i)
            {
                Fixed64 dot;
                FixedV3.Dot(ref vertices[i], ref right, out dot);
                if (dot < minX)
                {
                    minX = dot;
                    minXIndex = i;
                }
                else if (dot > maxX)
                {
                    maxX = dot;
                    maxXIndex = i;
                }

                FixedV3.Dot(ref vertices[i], ref up, out dot);
                if (dot < minY)
                {
                    minY = dot;
                    minYIndex = i;
                }
                else if (dot > maxY)
                {
                    maxY = dot;
                    maxYIndex = i;
                }

                FixedV3.Dot(ref vertices[i], ref backward, out dot);
                if (dot < minZ)
                {
                    minZ = dot;
                    minZIndex = i;
                }
                else if (dot > maxZ)
                {
                    maxZ = dot;
                    maxZIndex = i;
                }
            }

            //Rather than transforming each axis independently (and doing three times as many operations as required), just get the 6 required values directly.
            FixedV3 positive, negative;
            TransformLocalExtremePoints(ref vertices[maxXIndex], ref vertices[maxYIndex], ref vertices[maxZIndex], ref o, out positive);
            TransformLocalExtremePoints(ref vertices[minXIndex], ref vertices[minYIndex], ref vertices[minZIndex], ref o, out negative);

            //The positive and negative vectors represent the X, Y and Z coordinates of the extreme points in world space along the world space axes.
            boundingBox.Max.X = shapeTransform.Position.X + positive.X + collisionMargin;
            boundingBox.Max.Y = shapeTransform.Position.Y + positive.Y + collisionMargin;
            boundingBox.Max.Z = shapeTransform.Position.Z + positive.Z + collisionMargin;

            boundingBox.Min.X = shapeTransform.Position.X + negative.X - collisionMargin;
            boundingBox.Min.Y = shapeTransform.Position.Y + negative.Y - collisionMargin;
            boundingBox.Min.Z = shapeTransform.Position.Z + negative.Z - collisionMargin;
        }


        public override void GetLocalExtremePointWithoutMargin(ref FixedV3 direction, out FixedV3 extremePoint)
        {
            Fixed64 max;
            FixedV3.Dot(ref vertices[0], ref direction, out max);
            int maxIndex = 0;
            for (int i = 1; i < vertices.Length; i++)
            {
                Fixed64 dot;
                FixedV3.Dot(ref vertices[i], ref direction, out dot);
                if (dot > max)
                {
                    max = dot;
                    maxIndex = i;
                }
            }
            extremePoint = vertices[maxIndex];
        }



        /// <summary>
        /// Retrieves an instance of an EntityCollidable that uses this EntityShape.  Mainly used by compound bodies.
        /// </summary>
        /// <returns>EntityCollidable that uses this shape.</returns>
        public override EntityCollidable GetCollidableInstance()
        {
            return new ConvexCollidable<ConvexHullShape>(this);
        }


    }
}

using System;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;

using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.CollisionShapes.ConvexShapes
{
    ///<summary>
    /// Convex shape with width, length, and height.
    ///</summary>
    public class BoxShape : ConvexShape
    {
        internal Fixed64 halfWidth;
        internal Fixed64 halfHeight;
        internal Fixed64 halfLength;


        /// <summary>
        /// Width of the box divided by two.
        /// </summary>
        public Fixed64 HalfWidth
        {
            get { return halfWidth; }
            set { halfWidth = value; OnShapeChanged(); }
        }

        /// <summary>
        /// Height of the box divided by two.
        /// </summary>
        public Fixed64 HalfHeight
        {
            get { return halfHeight; }
            set { halfHeight = value; OnShapeChanged(); }
        }

        /// <summary>
        /// Length of the box divided by two.
        /// </summary>
        public Fixed64 HalfLength
        {
            get { return halfLength; }
            set { halfLength = value; OnShapeChanged(); }
        }

        /// <summary>
        /// Width of the box.
        /// </summary>
        public Fixed64 Width
        {
            get { return halfWidth * F64.C2; }
            set { halfWidth = value * F64.C0p5; OnShapeChanged(); }
        }

        /// <summary>
        /// Height of the box.
        /// </summary>
        public Fixed64 Height
        {
            get { return halfHeight * F64.C2; }
            set { halfHeight = value * F64.C0p5; OnShapeChanged(); }
        }

        /// <summary>
        /// Length of the box.
        /// </summary>
        public Fixed64 Length
        {
            get { return halfLength * F64.C2; }
            set { halfLength = value * F64.C0p5; OnShapeChanged(); }
        }


        ///<summary>
        /// Constructs a new box shape.
        ///</summary>
        ///<param name="width">Width of the box.</param>
        ///<param name="height">Height of the box.</param>
        ///<param name="length">Length of the box.</param>
        public BoxShape(Fixed64 width, Fixed64 height, Fixed64 length)
        {
            halfWidth = width * F64.C0p5;
            halfHeight = height * F64.C0p5;
            halfLength = length * F64.C0p5;

            UpdateConvexShapeInfo(ComputeDescription(width, height, length, collisionMargin));
        }

        ///<summary>
        /// Constructs a new box shape from cached information.
        ///</summary>
        ///<param name="width">Width of the box.</param>
        ///<param name="height">Height of the box.</param>
        ///<param name="length">Length of the box.</param>
        /// <param name="description">Cached information about the shape. Assumed to be correct; no extra processing or validation is performed.</param>
        public BoxShape(Fixed64 width, Fixed64 height, Fixed64 length, ConvexShapeDescription description)
        {
            halfWidth = width * F64.C0p5;
            halfHeight = height * F64.C0p5;
            halfLength = length * F64.C0p5;

            UpdateConvexShapeInfo(description);
        }

        protected override void OnShapeChanged()
        {
            UpdateConvexShapeInfo(ComputeDescription(halfWidth, halfHeight, halfLength, collisionMargin));
            base.OnShapeChanged();
        }

        /// <summary>
        /// Computes a convex shape description for a BoxShape.
        /// </summary>
        ///<param name="width">Width of the box.</param>
        ///<param name="height">Height of the box.</param>
        ///<param name="length">Length of the box.</param>
        /// <param name="collisionMargin">Collision margin of the shape.</param>
        /// <returns>Description required to define a convex shape.</returns>
        public static ConvexShapeDescription ComputeDescription(Fixed64 width, Fixed64 height, Fixed64 length, Fixed64 collisionMargin)
        {
            ConvexShapeDescription description;
            description.EntityShapeVolume.Volume = width * height * length;

            Fixed64 widthSquared = width * width;
            Fixed64 heightSquared = height * height;
            Fixed64 lengthSquared = length * length;
			Fixed64 inv12 = F64.OneTwelfth;

            description.EntityShapeVolume.VolumeDistribution = new BEPUMatrix3x3();
            description.EntityShapeVolume.VolumeDistribution.M11 = (heightSquared + lengthSquared) * inv12;
            description.EntityShapeVolume.VolumeDistribution.M22 = (widthSquared + lengthSquared) * inv12;
            description.EntityShapeVolume.VolumeDistribution.M33 = (widthSquared + heightSquared) * inv12;

            description.MaximumRadius = F64.C0p5 * Fixed64.Sqrt(width * width + height * height + length * length);
            description.MinimumRadius = F64.C0p5 * MathHelper.Min(width, MathHelper.Min(height, length));

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
#if !WINDOWS
            boundingBox = new BoundingBox();
#endif

            BEPUMatrix3x3 o;
            BEPUMatrix3x3.CreateFromQuaternion(ref shapeTransform.Orientation, out o);
            //Sample the local directions from the orientation matrix, implicitly transposed.
            //Notice only three directions are used.  Due to box symmetry, 'left' is just -right.
            var right = new FixedV3(Fixed64.Sign(o.M11) * halfWidth, Fixed64.Sign(o.M21) * halfHeight, Fixed64.Sign(o.M31) * halfLength);

            var up = new FixedV3(Fixed64.Sign(o.M12) * halfWidth, Fixed64.Sign(o.M22) * halfHeight, Fixed64.Sign(o.M32) * halfLength);

            var backward = new FixedV3(Fixed64.Sign(o.M13) * halfWidth, Fixed64.Sign(o.M23) * halfHeight, Fixed64.Sign(o.M33) * halfLength);


            //Rather than transforming each axis independently (and doing three times as many operations as required), just get the 3 required values directly.
            FixedV3 offset;
            TransformLocalExtremePoints(ref right, ref up, ref backward, ref o, out offset);

            //The positive and negative vectors represent the X, Y and Z coordinates of the extreme points in world space along the world space axes.
            FixedV3.Add(ref shapeTransform.Position, ref offset, out boundingBox.Max);
            FixedV3.Subtract(ref shapeTransform.Position, ref offset, out boundingBox.Min);

        }


        ///<summary>
        /// Gets the extreme point of the shape in local space in a given direction.
        ///</summary>
        ///<param name="direction">Direction to find the extreme point in.</param>
        ///<param name="extremePoint">Extreme point on the shape.</param>
        public override void GetLocalExtremePointWithoutMargin(ref FixedV3 direction, out FixedV3 extremePoint)
        {
            extremePoint = new FixedV3(Fixed64.Sign(direction.X) * (halfWidth - collisionMargin), Fixed64.Sign(direction.Y) * (halfHeight - collisionMargin), Fixed64.Sign(direction.Z) * (halfLength - collisionMargin));
        }




        /// <summary>
        /// Gets the intersection between the box and the ray.
        /// </summary>
        /// <param name="ray">Ray to test against the box.</param>
        /// <param name="transform">Transform of the shape.</param>
        /// <param name="maximumLength">Maximum distance to travel in units of the direction vector's length.</param>
        /// <param name="hit">Hit data for the raycast, if any.</param>
        /// <returns>Whether or not the ray hit the target.</returns>
        public override bool RayTest(ref BEPURay ray, ref RigidTransform transform, Fixed64 maximumLength, out RayHit hit)
        {
            hit = new RayHit();

            FixedQuaternion conjugate;
            FixedQuaternion.Conjugate(ref transform.Orientation, out conjugate);
            FixedV3 localOrigin;
            FixedV3.Subtract(ref ray.Position, ref transform.Position, out localOrigin);
            FixedQuaternion.Transform(ref localOrigin, ref conjugate, out localOrigin);
            FixedV3 localDirection;
            FixedQuaternion.Transform(ref ray.Direction, ref conjugate, out localDirection);
            FixedV3 normal = Toolbox.ZeroVector;
            Fixed64 temp, tmin = F64.C0, tmax = maximumLength;

            if (Fixed64.Abs(localDirection.X) < Toolbox.Epsilon && (localOrigin.X < -halfWidth || localOrigin.X > halfWidth))
                return false;
            Fixed64 inverseDirection = F64.C1 / localDirection.X;
			// inverseDirection might be Infinity (Fix64.MaxValue), so use SafeMul here to handle overflow
            Fixed64 t1 = Fixed64.SafeMul((-halfWidth - localOrigin.X), inverseDirection);
            Fixed64 t2 = Fixed64.SafeMul((halfWidth - localOrigin.X), inverseDirection);
            var tempNormal = new FixedV3(-1, F64.C0, F64.C0);
            if (t1 > t2)
            {
                temp = t1;
                t1 = t2;
                t2 = temp;
                tempNormal *= -1;
            }
            temp = tmin;
            tmin = MathHelper.Max(tmin, t1);
            if (temp != tmin)
                normal = tempNormal;
            tmax = MathHelper.Min(tmax, t2);
            if (tmin > tmax)
                return false;
            if (Fixed64.Abs(localDirection.Y) < Toolbox.Epsilon && (localOrigin.Y < -halfHeight || localOrigin.Y > halfHeight))
                return false;
            inverseDirection = F64.C1 / localDirection.Y;
            t1 = Fixed64.SafeMul((-halfHeight - localOrigin.Y), inverseDirection);
            t2 = Fixed64.SafeMul((halfHeight - localOrigin.Y), inverseDirection);
            tempNormal = new FixedV3(F64.C0, -1, F64.C0);
            if (t1 > t2)
            {
                temp = t1;
                t1 = t2;
                t2 = temp;
                tempNormal *= -1;
            }
            temp = tmin;
            tmin = MathHelper.Max(tmin, t1);
            if (temp != tmin)
                normal = tempNormal;
            tmax = MathHelper.Min(tmax, t2);
            if (tmin > tmax)
                return false;
            if (Fixed64.Abs(localDirection.Z) < Toolbox.Epsilon && (localOrigin.Z < -halfLength || localOrigin.Z > halfLength))
                return false;
            inverseDirection = F64.C1 / localDirection.Z;
            t1 = Fixed64.SafeMul((-halfLength - localOrigin.Z), inverseDirection);
            t2 = Fixed64.SafeMul((halfLength - localOrigin.Z), inverseDirection);
            tempNormal = new FixedV3(F64.C0, F64.C0, -1);
            if (t1 > t2)
            {
                temp = t1;
                t1 = t2;
                t2 = temp;
                tempNormal *= -1;
            }
            temp = tmin;
            tmin = MathHelper.Max(tmin, t1);
            if (temp != tmin)
                normal = tempNormal;
            tmax = MathHelper.Min(tmax, t2);
            if (tmin > tmax)
                return false;
            hit.T = tmin;
            FixedV3.Multiply(ref ray.Direction, tmin, out hit.Location);
            FixedV3.Add(ref hit.Location, ref ray.Position, out hit.Location);
            FixedQuaternion.Transform(ref normal, ref transform.Orientation, out normal);
            hit.Normal = normal;
            return true;
        }

        /// <summary>
        /// Retrieves an instance of an EntityCollidable that uses this EntityShape.  Mainly used by compound bodies.
        /// </summary>
        /// <returns>EntityCollidable that uses this shape.</returns>
        public override EntityCollidable GetCollidableInstance()
        {
            return new ConvexCollidable<BoxShape>(this);
        }

    }
}

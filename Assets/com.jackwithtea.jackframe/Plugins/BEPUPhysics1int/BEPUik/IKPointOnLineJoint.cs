using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Keeps the anchor points on two bones at the same distance.
    /// </summary>
    public class IKPointOnLineJoint : IKJoint
    {
        /// <summary>
        /// Gets or sets the offset in connection A's local space from the center of mass to the anchor point of the line.
        /// </summary>
        public FixedV3 LocalLineAnchor;

        private FixedV3 localLineDirection;
        /// <summary>
        /// Gets or sets the direction of the line in connection A's local space.
        /// Must be unit length.
        /// </summary>
        public FixedV3 LocalLineDirection
        {
            get { return localLineDirection; }
            set
            {
                localLineDirection = value;
                ComputeRestrictedAxes();
            }
        }


        /// <summary>
        /// Gets or sets the offset in connection B's local space from the center of mass to the anchor point which will be kept on the line.
        /// </summary>
        public FixedV3 LocalAnchorB;

        /// <summary>
        /// Gets or sets the world space location of the line anchor attached to connection A.
        /// </summary>
        public FixedV3 LineAnchor
        {
            get { return ConnectionA.Position + FixedQuaternion.Transform(LocalLineAnchor, ConnectionA.Orientation); }
            set { LocalLineAnchor = FixedQuaternion.Transform(value - ConnectionA.Position, FixedQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the world space direction of the line attached to connection A.
        /// Must be unit length.
        /// </summary>
        public FixedV3 LineDirection
        {
            get { return FixedQuaternion.Transform(localLineDirection, ConnectionA.Orientation); }
            set { LocalLineDirection = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the offset in world space from the center of mass of connection B to the anchor point.
        /// </summary>
        public FixedV3 AnchorB
        {
            get { return ConnectionB.Position + FixedQuaternion.Transform(LocalAnchorB, ConnectionB.Orientation); }
            set { LocalAnchorB = FixedQuaternion.Transform(value - ConnectionB.Position, FixedQuaternion.Conjugate(ConnectionB.Orientation)); }
        }

        private FixedV3 localRestrictedAxis1, localRestrictedAxis2;
        void ComputeRestrictedAxes()
        {
            FixedV3 cross;
            FixedV3.Cross(ref localLineDirection, ref Toolbox.UpVector, out cross);
            Fixed64 lengthSquared = cross.LengthSquared();
            if (lengthSquared > Toolbox.Epsilon)
            {
                FixedV3.Divide(ref cross, Fixed64.Sqrt(lengthSquared), out localRestrictedAxis1);
            }
            else
            {
                //Oops! The direction is aligned with the up vector.
                FixedV3.Cross(ref localLineDirection, ref Toolbox.RightVector, out cross);
                FixedV3.Normalize(ref cross, out localRestrictedAxis1);
            }
            //Don't need to normalize this; cross product of two unit length perpendicular vectors.
            FixedV3.Cross(ref localRestrictedAxis1, ref localLineDirection, out localRestrictedAxis2);
        }

        /// <summary>
        /// Constructs a new point on line joint.
        /// </summary>
        /// <param name="connectionA">First bone connected by the joint.</param>
        /// <param name="connectionB">Second bone connected by the joint.</param>
        /// <param name="lineAnchor">Anchor point of the line attached to the first bone in world space.</param>
        /// <param name="lineDirection">Direction of the line attached to the first bone in world space. Must be unit length.</param>
        /// <param name="anchorB">Anchor point on the second bone in world space which tries to stay on connection A's line.</param>
        public IKPointOnLineJoint(Bone connectionA, Bone connectionB, FixedV3 lineAnchor, FixedV3 lineDirection, FixedV3 anchorB)
            : base(connectionA, connectionB)
        {
            LineAnchor = lineAnchor;
            LineDirection = lineDirection;
            AnchorB = anchorB;

        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {

            //Transform local stuff into world space
            FixedV3 worldRestrictedAxis1, worldRestrictedAxis2;
            FixedQuaternion.Transform(ref localRestrictedAxis1, ref ConnectionA.Orientation, out worldRestrictedAxis1);
            FixedQuaternion.Transform(ref localRestrictedAxis2, ref ConnectionA.Orientation, out worldRestrictedAxis2);

            FixedV3 worldLineAnchor;
            FixedQuaternion.Transform(ref LocalLineAnchor, ref ConnectionA.Orientation, out worldLineAnchor);
            FixedV3.Add(ref worldLineAnchor, ref ConnectionA.Position, out worldLineAnchor);
            FixedV3 lineDirection;
            FixedQuaternion.Transform(ref localLineDirection, ref ConnectionA.Orientation, out lineDirection);

            FixedV3 rB;
            FixedQuaternion.Transform(ref LocalAnchorB, ref ConnectionB.Orientation, out rB);
            FixedV3 worldPoint;
            FixedV3.Add(ref rB, ref ConnectionB.Position, out worldPoint);

            //Find the point on the line closest to the world point.
            FixedV3 offset;
            FixedV3.Subtract(ref worldPoint, ref worldLineAnchor, out offset);
            Fixed64 distanceAlongAxis;
            FixedV3.Dot(ref offset, ref lineDirection, out distanceAlongAxis);

            FixedV3 worldNearPoint;
            FixedV3.Multiply(ref lineDirection, distanceAlongAxis, out offset);
            FixedV3.Add(ref worldLineAnchor, ref offset, out worldNearPoint);
            FixedV3 rA;
            FixedV3.Subtract(ref worldNearPoint, ref ConnectionA.Position, out rA);

            //Error
            FixedV3 error3D;
            FixedV3.Subtract(ref worldPoint, ref worldNearPoint, out error3D);

            FixedV2 error;
            FixedV3.Dot(ref error3D, ref worldRestrictedAxis1, out error.X);
            FixedV3.Dot(ref error3D, ref worldRestrictedAxis2, out error.Y);

            velocityBias.X = errorCorrectionFactor * error.X;
            velocityBias.Y = errorCorrectionFactor * error.Y;


            //Set up the jacobians
            FixedV3 angularA1, angularA2, angularB1, angularB2;
            FixedV3.Cross(ref rA, ref worldRestrictedAxis1, out angularA1);
            FixedV3.Cross(ref rA, ref worldRestrictedAxis2, out angularA2);
            FixedV3.Cross(ref worldRestrictedAxis1, ref rB, out angularB1);
            FixedV3.Cross(ref worldRestrictedAxis2, ref rB, out angularB2);

            //Put all the 1x3 jacobians into a 3x3 matrix representation.
            linearJacobianA = new BEPUMatrix3x3
            {
                M11 = worldRestrictedAxis1.X,
                M12 = worldRestrictedAxis1.Y,
                M13 = worldRestrictedAxis1.Z,
                M21 = worldRestrictedAxis2.X,
                M22 = worldRestrictedAxis2.Y,
                M23 = worldRestrictedAxis2.Z
            };
            BEPUMatrix3x3.Negate(ref linearJacobianA, out linearJacobianB);

            angularJacobianA = new BEPUMatrix3x3
            {
                M11 = angularA1.X,
                M12 = angularA1.Y,
                M13 = angularA1.Z,
                M21 = angularA2.X,
                M22 = angularA2.Y,
                M23 = angularA2.Z
            };
            angularJacobianB = new BEPUMatrix3x3
            {
                M11 = angularB1.X,
                M12 = angularB1.Y,
                M13 = angularB1.Z,
                M21 = angularB2.X,
                M22 = angularB2.Y,
                M23 = angularB2.Z
            };
        }
    }
}

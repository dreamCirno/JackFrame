using System;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUik
{
    public class IKRevoluteJoint : IKJoint
    {
        private FixedV3 localFreeAxisA;
        /// <summary>
        /// Gets or sets the free axis in connection A's local space.
        /// Must be unit length.
        /// </summary>
        public FixedV3 LocalFreeAxisA
        {
            get { return localFreeAxisA; }
            set
            {
                localFreeAxisA = value;
                ComputeConstrainedAxes();
            }
        }

        private FixedV3 localFreeAxisB;
        /// <summary>
        /// Gets or sets the free axis in connection B's local space.
        /// Must be unit length.
        /// </summary>
        public FixedV3 LocalFreeAxisB
        {
            get { return localFreeAxisB; }
            set
            {
                localFreeAxisB = value;
                ComputeConstrainedAxes();
            }
        }



        /// <summary>
        /// Gets or sets the free axis attached to connection A in world space.
        /// This does not change the other connection's free axis.
        /// </summary>
        public FixedV3 WorldFreeAxisA
        {
            get { return FixedQuaternion.Transform(localFreeAxisA, ConnectionA.Orientation); }
            set
            {
                LocalFreeAxisA = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionA.Orientation));
            }
        }

        /// <summary>
        /// Gets or sets the free axis attached to connection B in world space.
        /// This does not change the other connection's free axis.
        /// </summary>
        public FixedV3 WorldFreeAxisB
        {
            get { return FixedQuaternion.Transform(localFreeAxisB, ConnectionB.Orientation); }
            set
            {
                LocalFreeAxisB = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionB.Orientation));
            }
        }

        private FixedV3 localConstrainedAxis1, localConstrainedAxis2;
        void ComputeConstrainedAxes()
        {
            FixedV3 worldAxisA = WorldFreeAxisA;
            FixedV3 error = FixedV3.Cross(worldAxisA, WorldFreeAxisB);
            Fixed64 lengthSquared = error.LengthSquared();
            FixedV3 worldConstrainedAxis1, worldConstrainedAxis2;
            //Find the first constrained axis.
            if (lengthSquared > Toolbox.Epsilon)
            {
                //The error direction can be used as the first axis!
                FixedV3.Divide(ref error, Fixed64.Sqrt(lengthSquared), out worldConstrainedAxis1);
            }
            else
            {
                //There's not enough error for it to be a good constrained axis.
                //We'll need to create the constrained axes arbitrarily.
                FixedV3.Cross(ref Toolbox.UpVector, ref worldAxisA, out worldConstrainedAxis1);
                lengthSquared = worldConstrainedAxis1.LengthSquared();
                if (lengthSquared > Toolbox.Epsilon)
                {
                    //The up vector worked!
                    FixedV3.Divide(ref worldConstrainedAxis1, Fixed64.Sqrt(lengthSquared), out worldConstrainedAxis1);
                }
                else
                {
                    //The up vector didn't work. Just try the right vector.
                    FixedV3.Cross(ref Toolbox.RightVector, ref worldAxisA, out worldConstrainedAxis1);
                    worldConstrainedAxis1.Normalize();
                }
            }
            //Don't have to normalize the second constraint axis; it's the cross product of two perpendicular normalized vectors.
            FixedV3.Cross(ref worldAxisA, ref worldConstrainedAxis1, out worldConstrainedAxis2);

            localConstrainedAxis1 = FixedQuaternion.Transform(worldConstrainedAxis1, FixedQuaternion.Conjugate(ConnectionA.Orientation));
            localConstrainedAxis2 = FixedQuaternion.Transform(worldConstrainedAxis2, FixedQuaternion.Conjugate(ConnectionA.Orientation));
        }

        /// <summary>
        /// Constructs a new orientation joint.
        /// Orientation joints can be used to simulate the angular portion of a hinge.
        /// Orientation joints allow rotation around only a single axis.
        /// </summary>
        /// <param name="connectionA">First entity connected in the orientation joint.</param>
        /// <param name="connectionB">Second entity connected in the orientation joint.</param>
        /// <param name="freeAxis">Axis allowed to rotate freely in world space.</param>
        public IKRevoluteJoint(Bone connectionA, Bone connectionB, FixedV3 freeAxis)
            : base(connectionA, connectionB)
        {
            WorldFreeAxisA = freeAxis;
            WorldFreeAxisB = freeAxis;
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            linearJacobianA = linearJacobianB = new BEPUMatrix3x3();

            //We know the one free axis. We need the two restricted axes. This amounts to completing the orthonormal basis.
            //We can grab one of the restricted axes using a cross product of the two world axes. This is not guaranteed
            //to be nonzero, so the normalization requires protection.

            FixedV3 worldAxisA, worldAxisB;
            FixedQuaternion.Transform(ref localFreeAxisA, ref ConnectionA.Orientation, out worldAxisA);
            FixedQuaternion.Transform(ref localFreeAxisB, ref ConnectionB.Orientation, out worldAxisB);

            FixedV3 error;
            FixedV3.Cross(ref worldAxisA, ref worldAxisB, out error);

            FixedV3 worldConstrainedAxis1, worldConstrainedAxis2;
            FixedQuaternion.Transform(ref localConstrainedAxis1, ref ConnectionA.Orientation, out worldConstrainedAxis1);
            FixedQuaternion.Transform(ref localConstrainedAxis2, ref ConnectionA.Orientation, out worldConstrainedAxis2);


            angularJacobianA = new BEPUMatrix3x3
            {
                M11 = worldConstrainedAxis1.X,
                M12 = worldConstrainedAxis1.Y,
                M13 = worldConstrainedAxis1.Z,
                M21 = worldConstrainedAxis2.X,
                M22 = worldConstrainedAxis2.Y,
                M23 = worldConstrainedAxis2.Z
            };
            BEPUMatrix3x3.Negate(ref angularJacobianA, out angularJacobianB);


            FixedV2 constraintSpaceError;
            FixedV3.Dot(ref error, ref worldConstrainedAxis1, out constraintSpaceError.X);
            FixedV3.Dot(ref error, ref worldConstrainedAxis2, out constraintSpaceError.Y);
            velocityBias.X = errorCorrectionFactor * constraintSpaceError.X;
            velocityBias.Y = errorCorrectionFactor * constraintSpaceError.Y;


        }
    }
}

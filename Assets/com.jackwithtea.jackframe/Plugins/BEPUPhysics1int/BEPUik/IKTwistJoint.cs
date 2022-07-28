using System;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Prevents two bones from twisting relative to each other..
    /// </summary>
    public class IKTwistJoint : IKJoint
    {
        /// <summary>
        /// Gets or sets the axis attached to ConnectionA in its local space.
        /// Must be unit length and perpendicular to LocalMeasurementAxisA.
        /// </summary>
        public FixedV3 LocalAxisA;
        /// <summary>
        /// Gets or sets the axis attached to ConnectionB in its local space.
        /// Must be unit length and perpendicular to LocalMeasurementAxisB.
        /// </summary>
        public FixedV3 LocalAxisB;

        /// <summary>
        /// Gets or sets the measurement axis attached to connection A.
        /// Must be unit length and perpendicular to LocalAxisA.
        /// </summary>
        public FixedV3 LocalMeasurementAxisA;
        /// <summary>
        /// Gets or sets the measurement axis attached to connection B.
        /// Must be unit length and perpendicular to LocalAxisB.
        /// </summary>
        public FixedV3 LocalMeasurementAxisB;

        /// <summary>
        /// Gets or sets the axis attached to ConnectionA in world space.
        /// Must be unit length and perpendicular to MeasurementAxisA.
        /// </summary>
        public FixedV3 AxisA
        {
            get { return FixedQuaternion.Transform(LocalAxisA, ConnectionA.Orientation); }
            set { LocalAxisA = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the axis attached to ConnectionB in world space.
        /// Must be unit length and perpendicular to MeasurementAxisB.
        /// </summary>
        public FixedV3 AxisB
        {
            get { return FixedQuaternion.Transform(LocalAxisB, ConnectionB.Orientation); }
            set { LocalAxisB = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionB.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the measurement axis attached to ConnectionA in world space.
        /// This axis is compared against the other connection's measurement axis to determine the twist.
        /// Must be unit length and perpendicular to AxisA.
        /// </summary>
        public FixedV3 MeasurementAxisA
        {
            get { return FixedQuaternion.Transform(LocalMeasurementAxisA, ConnectionA.Orientation); }
            set { LocalMeasurementAxisA = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the measurement axis attached to ConnectionB in world space.
        /// This axis is compared against the other connection's measurement axis to determine the twist.
        /// Must be unit length and perpendicular to AxisB.
        /// </summary>
        public FixedV3 MeasurementAxisB
        {
            get { return FixedQuaternion.Transform(LocalMeasurementAxisB, ConnectionB.Orientation); }
            set { LocalMeasurementAxisB = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionB.Orientation)); }
        }

        /// <summary>
        /// Automatically computes the measurement axes for the current local axes.
        /// The current relative state of the entities will be considered 0 twist angle.
        /// </summary>
        public void ComputeMeasurementAxes()
        {
            FixedV3 axisA, axisB;
            FixedQuaternion.Transform(ref LocalAxisA, ref ConnectionA.Orientation, out axisA);
            FixedQuaternion.Transform(ref LocalAxisB, ref ConnectionB.Orientation, out axisB);
            //Pick an axis perpendicular to axisA to use as the measurement axis.
            FixedV3 worldMeasurementAxisA;
            FixedV3.Cross(ref Toolbox.UpVector, ref axisA, out worldMeasurementAxisA);
            Fixed64 lengthSquared = worldMeasurementAxisA.LengthSquared();
            if (lengthSquared > Toolbox.Epsilon)
            {
                FixedV3.Divide(ref worldMeasurementAxisA, Fixed64.Sqrt(lengthSquared), out worldMeasurementAxisA);
            }
            else
            {
                //Oops! It was parallel to the up vector. Just try again with the right vector.
                FixedV3.Cross(ref Toolbox.RightVector, ref axisA, out worldMeasurementAxisA);
                worldMeasurementAxisA.Normalize();
            }
            //Attach the measurement axis to entity B.
            //'Push' A's axis onto B by taking into account the swing transform.
            FixedQuaternion alignmentRotation;
            FixedQuaternion.GetQuaternionBetweenNormalizedVectors(ref axisA, ref axisB, out alignmentRotation);
            FixedV3 worldMeasurementAxisB;
            FixedQuaternion.Transform(ref worldMeasurementAxisA, ref alignmentRotation, out worldMeasurementAxisB);
            //Plop them on!
            MeasurementAxisA = worldMeasurementAxisA;
            MeasurementAxisB = worldMeasurementAxisB;

        }


        /// <summary>
        /// Builds a new twist limit. Prevents two bones from rotating beyond a certain angle away from each other as measured by attaching an axis to each connected bone.
        /// </summary>
        /// <param name="connectionA">First connection of the limit.</param>
        /// <param name="connectionB">Second connection of the limit.</param>
        /// <param name="axisA">Axis attached to connectionA in world space.</param>
        /// <param name="axisB">Axis attached to connectionB in world space.</param>
        public IKTwistJoint(Bone connectionA, Bone connectionB, FixedV3 axisA, FixedV3 axisB)
            : base(connectionA, connectionB)
        {
            AxisA = axisA;
            AxisB = axisB;

            ComputeMeasurementAxes();
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {

            //This constraint doesn't consider linear motion.
            linearJacobianA = linearJacobianB = new BEPUMatrix3x3();

            //Compute the world axes.
            FixedV3 axisA, axisB;
            FixedQuaternion.Transform(ref LocalAxisA, ref ConnectionA.Orientation, out axisA);
            FixedQuaternion.Transform(ref LocalAxisB, ref ConnectionB.Orientation, out axisB);

            FixedV3 twistMeasureAxisA, twistMeasureAxisB;
            FixedQuaternion.Transform(ref LocalMeasurementAxisA, ref ConnectionA.Orientation, out twistMeasureAxisA);
            FixedQuaternion.Transform(ref LocalMeasurementAxisB, ref ConnectionB.Orientation, out twistMeasureAxisB);

            //Compute the shortest rotation to bring axisB into alignment with axisA.
            FixedQuaternion alignmentRotation;
            FixedQuaternion.GetQuaternionBetweenNormalizedVectors(ref axisB, ref axisA, out alignmentRotation);

            //Transform the measurement axis on B by the alignment quaternion.
            FixedQuaternion.Transform(ref twistMeasureAxisB, ref alignmentRotation, out twistMeasureAxisB);

            //We can now compare the angle between the twist axes.
            Fixed64 error;
            FixedV3.Dot(ref twistMeasureAxisA, ref twistMeasureAxisB, out error);
            error = Fixed64.Acos(MathHelper.Clamp(error, -1, F64.C1));
            FixedV3 cross;
            FixedV3.Cross(ref twistMeasureAxisA, ref twistMeasureAxisB, out cross);
            Fixed64 dot;
            FixedV3.Dot(ref cross, ref axisA, out dot);
            if (dot < F64.C0)
                error = -error;

            //Compute the bias based upon the error.
            velocityBias = new FixedV3(errorCorrectionFactor * error, F64.C0, F64.C0);

            //We can't just use the axes directly as jacobians. Consider 'cranking' one object around the other.
            FixedV3 jacobian;
            FixedV3.Add(ref axisA, ref axisB, out jacobian);
            Fixed64 lengthSquared = jacobian.LengthSquared();
            if (lengthSquared > Toolbox.Epsilon)
            {
                FixedV3.Divide(ref jacobian, Fixed64.Sqrt(lengthSquared), out jacobian);
            }
            else
            {
                //The constraint is in an invalid configuration. Just ignore it.
                jacobian = new FixedV3();
            }

            angularJacobianA = new BEPUMatrix3x3 { M11 = jacobian.X, M12 = jacobian.Y, M13 = jacobian.Z };
            angularJacobianB = new BEPUMatrix3x3 { M11 = -jacobian.X, M12 = -jacobian.Y, M13 = -jacobian.Z };




        }
    }
}

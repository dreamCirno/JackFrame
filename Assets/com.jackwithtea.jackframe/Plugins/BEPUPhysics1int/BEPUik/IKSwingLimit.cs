using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Prevents two bones from rotating beyond a certain angle away from each other as measured by attaching an axis to each connected bone.
    /// </summary>
    public class IKSwingLimit : IKLimit
    {
        /// <summary>
        /// Gets or sets the axis attached to ConnectionA in its local space.
        /// </summary>
        public FixedV3 LocalAxisA;
        /// <summary>
        /// Gets or sets the axis attached to ConnectionB in its local space.
        /// </summary>
        public FixedV3 LocalAxisB;

        /// <summary>
        /// Gets or sets the axis attached to ConnectionA in world space.
        /// </summary>
        public FixedV3 AxisA
        {
            get { return FixedQuaternion.Transform(LocalAxisA, ConnectionA.Orientation); }
            set { LocalAxisA = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        ///  Gets or sets the axis attached to ConnectionB in world space.
        /// </summary>
        public FixedV3 AxisB
        {
            get { return FixedQuaternion.Transform(LocalAxisB, ConnectionB.Orientation); }
            set { LocalAxisB = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionB.Orientation)); }
        }

        private Fixed64 maximumAngle;
        /// <summary>
        /// Gets or sets the maximum angle between the two axes allowed by the constraint.
        /// </summary>
        public Fixed64 MaximumAngle
        {
            get { return maximumAngle; }
            set { maximumAngle = MathHelper.Max(F64.C0, value); }
        }


        /// <summary>
        /// Builds a new swing limit. Prevents two bones from rotating beyond a certain angle away from each other as measured by attaching an axis to each connected bone.
        /// </summary>
        /// <param name="connectionA">First connection of the limit.</param>
        /// <param name="connectionB">Second connection of the limit.</param>
        /// <param name="axisA">Axis attached to connectionA in world space.</param>
        /// <param name="axisB">Axis attached to connectionB in world space.</param>
        /// <param name="maximumAngle">Maximum angle allowed between connectionA's axis and connectionB's axis.</param>
        public IKSwingLimit(Bone connectionA, Bone connectionB, FixedV3 axisA, FixedV3 axisB, Fixed64 maximumAngle)
            : base(connectionA, connectionB)
        {
            AxisA = axisA;
            AxisB = axisB;
            MaximumAngle = maximumAngle;
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {

            //This constraint doesn't consider linear motion.
            linearJacobianA = linearJacobianB = new BEPUMatrix3x3();

            //Compute the world axes.
            FixedV3 axisA, axisB;
            FixedQuaternion.Transform(ref LocalAxisA, ref ConnectionA.Orientation, out axisA);
            FixedQuaternion.Transform(ref LocalAxisB, ref ConnectionB.Orientation, out axisB);

            Fixed64 dot;
            FixedV3.Dot(ref axisA, ref axisB, out dot);

            //Yes, we could avoid this acos here. Performance is not the highest goal of this system; the less tricks used, the easier it is to understand.
			// TODO investigate performance
            Fixed64 angle = Fixed64.Acos(MathHelper.Clamp(dot, -1, F64.C1));

            //One angular DOF is constrained by this limit.
            FixedV3 hingeAxis;
            FixedV3.Cross(ref axisA, ref axisB, out hingeAxis);

            angularJacobianA = new BEPUMatrix3x3 { M11 = hingeAxis.X, M12 = hingeAxis.Y, M13 = hingeAxis.Z };
            angularJacobianB = new BEPUMatrix3x3 { M11 = -hingeAxis.X, M12 = -hingeAxis.Y, M13 = -hingeAxis.Z };

            //Note how we've computed the jacobians despite the limit being potentially inactive.
            //This is to enable 'speculative' limits.
            if (angle >= maximumAngle)
            {
                velocityBias = new FixedV3(errorCorrectionFactor * (angle - maximumAngle), F64.C0, F64.C0);
            }
            else
            {
                //The constraint is not yet violated. But, it may be- allow only as much motion as could occur without violating the constraint.
                //Limits can't 'pull,' so this will not result in erroneous sticking.
                velocityBias = new FixedV3(angle - maximumAngle, F64.C0, F64.C0);
            }


        }
    }
}

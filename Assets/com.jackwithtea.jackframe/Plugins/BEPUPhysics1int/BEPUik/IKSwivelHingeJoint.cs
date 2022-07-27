using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    public class IKSwivelHingeJoint : IKJoint
    {
        /// <summary>
        /// Gets or sets the free hinge axis attached to connection A in its local space.
        /// </summary>
        public FixedV3 LocalHingeAxis;
        /// <summary>
        /// Gets or sets the free twist axis attached to connection B in its local space.
        /// </summary>
        public FixedV3 LocalTwistAxis;


        /// <summary>
        /// Gets or sets the free hinge axis attached to connection A in world space.
        /// </summary>
        public FixedV3 WorldHingeAxis
        {
            get { return FixedQuaternion.Transform(LocalHingeAxis, ConnectionA.Orientation); }
            set
            {
                LocalHingeAxis = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionA.Orientation));
            }
        }

        /// <summary>
        /// Gets or sets the free twist axis attached to connection B in world space.
        /// </summary>
        public FixedV3 WorldTwistAxis
        {
            get { return FixedQuaternion.Transform(LocalTwistAxis, ConnectionB.Orientation); }
            set
            {
                LocalTwistAxis = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionB.Orientation));
            }
        }

        /// <summary>
        /// Constructs a new constraint which allows relative angular motion around a hinge axis and a twist axis.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        /// <param name="worldHingeAxis">Hinge axis attached to connectionA.
        /// The connected bone will be able to rotate around this axis relative to each other.</param>
        /// <param name="worldTwistAxis">Twist axis attached to connectionB.
        /// The connected bones will be able to rotate around this axis relative to each other.</param>
        public IKSwivelHingeJoint(Bone connectionA, Bone connectionB, FixedV3 worldHingeAxis, FixedV3 worldTwistAxis)
            : base(connectionA, connectionB)
        {
            WorldHingeAxis = worldHingeAxis;
            WorldTwistAxis = worldTwistAxis;
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            linearJacobianA = linearJacobianB = new BEPUMatrix3x3();


            //There are two free axes and one restricted axis.
            //The constraint attempts to keep the hinge axis attached to connection A and the twist axis attached to connection B perpendicular to each other.
            //The restricted axis is the cross product between the twist and hinge axes.

            FixedV3 worldTwistAxis, worldHingeAxis;
            FixedQuaternion.Transform(ref LocalHingeAxis, ref ConnectionA.Orientation, out worldHingeAxis);
            FixedQuaternion.Transform(ref LocalTwistAxis, ref ConnectionB.Orientation, out worldTwistAxis);

            FixedV3 restrictedAxis;
            FixedV3.Cross(ref worldHingeAxis, ref worldTwistAxis, out restrictedAxis);
            //Attempt to normalize the restricted axis.
            Fixed64 lengthSquared = restrictedAxis.LengthSquared();
            if (lengthSquared > Toolbox.Epsilon)
            {
                FixedV3.Divide(ref restrictedAxis, Fixed64.Sqrt(lengthSquared), out restrictedAxis);
            }
            else
            {
                restrictedAxis = new FixedV3();
            }


            angularJacobianA = new BEPUMatrix3x3
              {
                  M11 = restrictedAxis.X,
                  M12 = restrictedAxis.Y,
                  M13 = restrictedAxis.Z,
              };
            BEPUMatrix3x3.Negate(ref angularJacobianA, out angularJacobianB);

            Fixed64 error;
            FixedV3.Dot(ref worldHingeAxis, ref worldTwistAxis, out error);
            error = Fixed64.Acos(MathHelper.Clamp(error, -1, F64.C1)) - MathHelper.PiOver2;

            velocityBias = new FixedV3(errorCorrectionFactor * error, F64.C0, F64.C0);


        }
    }
}

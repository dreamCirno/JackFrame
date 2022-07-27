using System;
using BEPUPhysics1int;

namespace BEPUik
{
    public class SingleBoneRevoluteConstraint : SingleBoneConstraint
    {
        private FixedV3 freeAxis;
        private FixedV3 constrainedAxis1;
        private FixedV3 constrainedAxis2;

        /// <summary>
        /// Gets or sets the direction to constrain the bone free axis to.
        /// </summary>
        public FixedV3 FreeAxis
        {
            get { return freeAxis; }
            set
            {
                freeAxis = value;
                constrainedAxis1 = FixedV3.Cross(freeAxis, FixedV3.Up);
                if (constrainedAxis1.LengthSquared() < Toolbox.Epsilon)
                {
                    constrainedAxis1 = FixedV3.Cross(freeAxis, FixedV3.Right);
                }
                constrainedAxis1.Normalize();
                constrainedAxis2 = FixedV3.Cross(freeAxis, constrainedAxis1);
            }
        }


        /// <summary>
        /// Axis of allowed rotation in the bone's local space.
        /// </summary>
        public FixedV3 BoneLocalFreeAxis;

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
 

            linearJacobian = new BEPUMatrix3x3();

            FixedV3 boneAxis;
            FixedQuaternion.Transform(ref BoneLocalFreeAxis, ref TargetBone.Orientation, out boneAxis);


            angularJacobian = new BEPUMatrix3x3
            {
                M11 = constrainedAxis1.X,
                M12 = constrainedAxis1.Y,
                M13 = constrainedAxis1.Z,
                M21 = constrainedAxis2.X,
                M22 = constrainedAxis2.Y,
                M23 = constrainedAxis2.Z
            };


            FixedV3 error;
            FixedV3.Cross(ref boneAxis, ref freeAxis, out error);
            FixedV2 constraintSpaceError;
            FixedV3.Dot(ref error, ref constrainedAxis1, out constraintSpaceError.X);
            FixedV3.Dot(ref error, ref constrainedAxis2, out constraintSpaceError.Y);
            velocityBias.X = errorCorrectionFactor * constraintSpaceError.X;
            velocityBias.Y = errorCorrectionFactor * constraintSpaceError.Y;


        }


    }
}

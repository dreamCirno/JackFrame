using System;
using BEPUPhysics1int;

namespace BEPUik
{
    public class SingleBoneAngularPlaneConstraint : SingleBoneConstraint
    {
        /// <summary>
        /// Gets or sets normal of the plane which the bone's axis will be constrained to..
        /// </summary>
        public FixedV3 PlaneNormal;



        /// <summary>
        /// Axis to constrain to the plane in the bone's local space.
        /// </summary>
        public FixedV3 BoneLocalAxis;

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
 

            linearJacobian = new BEPUMatrix3x3();

            FixedV3 boneAxis;
            FixedQuaternion.Transform(ref BoneLocalAxis, ref TargetBone.Orientation, out boneAxis);

            FixedV3 jacobian;
            FixedV3.Cross(ref boneAxis, ref PlaneNormal, out jacobian);

            angularJacobian = new BEPUMatrix3x3
            {
                M11 = jacobian.X,
                M12 = jacobian.Y,
                M13 = jacobian.Z,
            };


            FixedV3.Dot(ref boneAxis, ref PlaneNormal, out velocityBias.X);
            velocityBias.X = -errorCorrectionFactor * velocityBias.X;


        }


    }
}

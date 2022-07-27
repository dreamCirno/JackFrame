using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUik
{
    public class SingleBoneAngularMotor : SingleBoneConstraint
    {
        /// <summary>
        /// Gets or sets the target orientation to apply to the target bone.
        /// </summary>
        public FixedQuaternion TargetOrientation;

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            linearJacobian = new BEPUMatrix3x3();
            angularJacobian = BEPUMatrix3x3.Identity;

            //Error is in world space. It gets projected onto the jacobians later.
            FixedQuaternion errorQuaternion;
            FixedQuaternion.Conjugate(ref TargetBone.Orientation, out errorQuaternion);
            FixedQuaternion.Multiply(ref TargetOrientation, ref errorQuaternion, out errorQuaternion);
            Fixed64 angle;
            FixedV3 angularError;
            FixedQuaternion.GetAxisAngleFromQuaternion(ref errorQuaternion, out angularError, out angle);
            FixedV3.Multiply(ref angularError, angle, out angularError);

            //This is equivalent to projecting the error onto the angular jacobian. The angular jacobian just happens to be the identity matrix!
            FixedV3.Multiply(ref angularError, errorCorrectionFactor, out velocityBias);
        }


    }
}

using BEPUutilities;

namespace BEPUik
{
    public class SingleBoneLinearMotor : SingleBoneConstraint
    {
        /// <summary>
        /// Gets or sets the target position to apply to the target bone.
        /// </summary>
        public FixedV3 TargetPosition;

        /// <summary>
        /// Gets or sets the offset in the bone's local space to the point which will be pulled towards the target position.
        /// </summary>
        public FixedV3 LocalOffset;


        public FixedV3 Offset
        {
            get { return FixedQuaternion.Transform(LocalOffset, TargetBone.Orientation); }
            set { LocalOffset = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(TargetBone.Orientation)); }
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            linearJacobian = BEPUMatrix3x3.Identity;
            FixedV3 r;
            FixedQuaternion.Transform(ref LocalOffset, ref TargetBone.Orientation, out r);
            BEPUMatrix3x3.CreateCrossProduct(ref r, out angularJacobian);
            //Transposing a skew symmetric matrix is equivalent to negating it.
            BEPUMatrix3x3.Transpose(ref angularJacobian, out angularJacobian);

            FixedV3 worldPosition;
            FixedV3.Add(ref TargetBone.Position, ref r, out worldPosition);

            //Error is in world space.
            FixedV3 linearError;
            FixedV3.Subtract(ref TargetPosition, ref worldPosition, out linearError);
            //This is equivalent to projecting the error onto the linear jacobian. The linear jacobian just happens to be the identity matrix!
            FixedV3.Multiply(ref linearError, errorCorrectionFactor, out velocityBias);
        }


    }
}

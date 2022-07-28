using BEPUPhysics1int;

namespace BEPUik
{
    //Keeps the anchors from two connections near each other.
    public class IKBallSocketJoint : IKJoint
    {
        /// <summary>
        /// Gets or sets the offset in connection A's local space from the center of mass to the anchor point.
        /// </summary>
        public FixedV3 LocalOffsetA;
        /// <summary>
        /// Gets or sets the offset in connection B's local space from the center of mass to the anchor point.
        /// </summary>
        public FixedV3 LocalOffsetB;

        /// <summary>
        /// Gets or sets the offset in world space from the center of mass of connection A to the anchor point.
        /// </summary>
        public FixedV3 OffsetA
        {
            get { return FixedQuaternion.Transform(LocalOffsetA, ConnectionA.Orientation); }
            set { LocalOffsetA = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the offset in world space from the center of mass of connection B to the anchor point.
        /// </summary>
        public FixedV3 OffsetB
        {
            get { return FixedQuaternion.Transform(LocalOffsetB, ConnectionB.Orientation); }
            set { LocalOffsetB = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionB.Orientation)); }
        }

        /// <summary>
        /// Builds a ball socket joint.
        /// </summary>
        /// <param name="connectionA">First connection in the pair.</param>
        /// <param name="connectionB">Second connection in the pair.</param>
        /// <param name="anchor">World space anchor location used to initialize the local anchors.</param>
        public IKBallSocketJoint(Bone connectionA, Bone connectionB, FixedV3 anchor)
            : base(connectionA, connectionB)
        {
            OffsetA = anchor - ConnectionA.Position;
            OffsetB = anchor - ConnectionB.Position;
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            linearJacobianA = BEPUMatrix3x3.Identity;
            //The jacobian entries are is [ La, Aa, -Lb, -Ab ] because the relative velocity is computed using A-B. So, negate B's jacobians!
            linearJacobianB = new BEPUMatrix3x3 { M11 = -1, M22 = -1, M33 = -1 };
            FixedV3 rA;
            FixedQuaternion.Transform(ref LocalOffsetA, ref ConnectionA.Orientation, out rA);
            BEPUMatrix3x3.CreateCrossProduct(ref rA, out angularJacobianA);
            //Transposing a skew-symmetric matrix is equivalent to negating it.
            BEPUMatrix3x3.Transpose(ref angularJacobianA, out angularJacobianA);

            FixedV3 worldPositionA;
            FixedV3.Add(ref ConnectionA.Position, ref rA, out worldPositionA);

            FixedV3 rB;
            FixedQuaternion.Transform(ref LocalOffsetB, ref ConnectionB.Orientation, out rB);
            BEPUMatrix3x3.CreateCrossProduct(ref rB, out angularJacobianB);

            FixedV3 worldPositionB;
            FixedV3.Add(ref ConnectionB.Position, ref rB, out worldPositionB);

            FixedV3 linearError;
            FixedV3.Subtract(ref worldPositionB, ref worldPositionA, out linearError);
            FixedV3.Multiply(ref linearError, errorCorrectionFactor, out velocityBias);

        }
    }
}

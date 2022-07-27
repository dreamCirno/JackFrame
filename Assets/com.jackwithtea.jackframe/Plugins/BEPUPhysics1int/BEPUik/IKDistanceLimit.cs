using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Tries to keep the anchor points on two bones within an allowed range of distances.
    /// </summary>
    public class IKDistanceLimit : IKLimit
    {
        /// <summary>
        /// Gets or sets the offset in connection A's local space from the center of mass to the anchor point.
        /// </summary>
        public FixedV3 LocalAnchorA;
        /// <summary>
        /// Gets or sets the offset in connection B's local space from the center of mass to the anchor point.
        /// </summary>
        public FixedV3 LocalAnchorB;

        /// <summary>
        /// Gets or sets the offset in world space from the center of mass of connection A to the anchor point.
        /// </summary>
        public FixedV3 AnchorA
        {
            get { return ConnectionA.Position + FixedQuaternion.Transform(LocalAnchorA, ConnectionA.Orientation); }
            set { LocalAnchorA = FixedQuaternion.Transform(value - ConnectionA.Position, FixedQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the offset in world space from the center of mass of connection B to the anchor point.
        /// </summary>
        public FixedV3 AnchorB
        {
            get { return ConnectionB.Position + FixedQuaternion.Transform(LocalAnchorB, ConnectionB.Orientation); }
            set { LocalAnchorB = FixedQuaternion.Transform(value - ConnectionB.Position, FixedQuaternion.Conjugate(ConnectionB.Orientation)); }
        }

        private Fixed64 minimumDistance;
        /// <summary>
        /// Gets or sets the minimum distance that the joint connections should be kept from each other.
        /// </summary>
        public Fixed64 MinimumDistance
        {
            get { return minimumDistance; }
            set { minimumDistance = MathHelper.Max(F64.C0, value); }
        }

         private Fixed64 maximumDistance;
        /// <summary>
        /// Gets or sets the maximum distance that the joint connections should be kept from each other.
        /// </summary>
        public Fixed64 MaximumDistance
        {
            get { return maximumDistance; }
            set { maximumDistance = MathHelper.Max(F64.C0, value); }
        }

        /// <summary>
        /// Constructs a new distance joint.
        /// </summary>
        /// <param name="connectionA">First bone connected by the joint.</param>
        /// <param name="connectionB">Second bone connected by the joint.</param>
        /// <param name="anchorA">Anchor point on the first bone in world space.</param>
        /// <param name="anchorB">Anchor point on the second bone in world space.</param>
        /// <param name="minimumDistance">Minimum distance that the joint connections should be kept from each other.</param>
        /// <param name="maximumDistance">Maximum distance that the joint connections should be kept from each other.</param>
        public IKDistanceLimit(Bone connectionA, Bone connectionB, FixedV3 anchorA, FixedV3 anchorB, Fixed64 minimumDistance, Fixed64 maximumDistance)
            : base(connectionA, connectionB)
        {
            AnchorA = anchorA;
            AnchorB = anchorB;
            MinimumDistance = minimumDistance;
            MaximumDistance = maximumDistance;
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            //Transform the anchors and offsets into world space.
            FixedV3 offsetA, offsetB;
            FixedQuaternion.Transform(ref LocalAnchorA, ref ConnectionA.Orientation, out offsetA);
            FixedQuaternion.Transform(ref LocalAnchorB, ref ConnectionB.Orientation, out offsetB);
            FixedV3 anchorA, anchorB;
            FixedV3.Add(ref ConnectionA.Position, ref offsetA, out anchorA);
            FixedV3.Add(ref ConnectionB.Position, ref offsetB, out anchorB);

            //Compute the distance.
            FixedV3 separation;
            FixedV3.Subtract(ref anchorB, ref anchorA, out separation);
            Fixed64 currentDistance = separation.Length();

            //Compute jacobians
            FixedV3 linearA;
#if !WINDOWS
            linearA = new FixedV3();
#endif
            if (currentDistance > Toolbox.Epsilon)
            {
                linearA.X = separation.X / currentDistance;
                linearA.Y = separation.Y / currentDistance;
                linearA.Z = separation.Z / currentDistance;

                if (currentDistance > maximumDistance)
                {
                    //We are exceeding the maximum limit.
                    velocityBias = new FixedV3(errorCorrectionFactor * (currentDistance - maximumDistance), F64.C0, F64.C0);
                }
                else if (currentDistance < minimumDistance)
                {
                    //We are exceeding the minimum limit.
                    velocityBias = new FixedV3(errorCorrectionFactor * (minimumDistance - currentDistance), F64.C0, F64.C0);
                    //The limit can only push in one direction. Flip the jacobian!
                    FixedV3.Negate(ref linearA, out linearA);
                }
                else if (currentDistance - minimumDistance > (maximumDistance - minimumDistance) * F64.C0p5)
                {
                    //The objects are closer to hitting the maximum limit.
                    velocityBias = new FixedV3(currentDistance - maximumDistance, F64.C0, F64.C0);
                }
                else
                {
                    //The objects are closer to hitting the minimum limit.
                    velocityBias = new FixedV3(minimumDistance - currentDistance, F64.C0, F64.C0);
                    //The limit can only push in one direction. Flip the jacobian!
                    FixedV3.Negate(ref linearA, out linearA);
                }
            }
            else
            {
                velocityBias = new FixedV3();
                linearA = new FixedV3();
            }

            FixedV3 angularA, angularB;
            FixedV3.Cross(ref offsetA, ref linearA, out angularA);
            //linearB = -linearA, so just swap the cross product order.
            FixedV3.Cross(ref linearA, ref offsetB, out angularB);

            //Put all the 1x3 jacobians into a 3x3 matrix representation.
            linearJacobianA = new BEPUMatrix3x3 { M11 = linearA.X, M12 = linearA.Y, M13 = linearA.Z };
            linearJacobianB = new BEPUMatrix3x3 { M11 = -linearA.X, M12 = -linearA.Y, M13 = -linearA.Z };
            angularJacobianA = new BEPUMatrix3x3 { M11 = angularA.X, M12 = angularA.Y, M13 = angularA.Z };
            angularJacobianB = new BEPUMatrix3x3 { M11 = angularB.X, M12 = angularB.Y, M13 = angularB.Z };

        }
    }
}

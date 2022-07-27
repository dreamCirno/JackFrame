using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Keeps the anchor points on two bones at the same distance.
    /// </summary>
    public class IKDistanceJoint : IKJoint
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
        /// Gets or sets the offset in world space from the center of mass of connection A to the anchor point.
        /// </summary>
        public FixedV3 AnchorB
        {
            get { return ConnectionB.Position + FixedQuaternion.Transform(LocalAnchorB, ConnectionB.Orientation); }
            set { LocalAnchorB = FixedQuaternion.Transform(value - ConnectionB.Position, FixedQuaternion.Conjugate(ConnectionB.Orientation)); }
        }

        private Fixed64 distance;
        /// <summary>
        /// Gets or sets the distance that the joint connections should be kept from each other.
        /// </summary>
        public Fixed64 Distance
        {
            get { return distance; }
            set { distance = MathHelper.Max(F64.C0, value); }
        }

        /// <summary>
        /// Constructs a new distance joint.
        /// </summary>
        /// <param name="connectionA">First bone connected by the joint.</param>
        /// <param name="connectionB">Second bone connected by the joint.</param>
        /// <param name="anchorA">Anchor point on the first bone in world space.</param>
        /// <param name="anchorB">Anchor point on the second bone in world space.</param>
        public IKDistanceJoint(Bone connectionA, Bone connectionB, FixedV3 anchorA, FixedV3 anchorB)
            : base(connectionA, connectionB)
        {
            AnchorA = anchorA;
            AnchorB = anchorB;
            FixedV3.Distance(ref anchorA, ref anchorB, out distance);
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

                velocityBias = new FixedV3(errorCorrectionFactor * (currentDistance - distance), F64.C0, F64.C0);
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

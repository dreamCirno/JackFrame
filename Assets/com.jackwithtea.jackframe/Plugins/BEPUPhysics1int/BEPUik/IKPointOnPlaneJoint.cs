using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Keeps an anchor point on one bone on a plane defined by another bone.
    /// </summary>
    public class IKPointOnPlaneJoint : IKJoint
    {
        /// <summary>
        /// Gets or sets the offset in connection A's local space from the center of mass to the anchor point of the line.
        /// </summary>
        public FixedV3 LocalPlaneAnchor;

        /// <summary>
        /// Gets or sets the direction of the line in connection A's local space.
        /// Must be unit length.
        /// </summary>
        public FixedV3 LocalPlaneNormal;

        /// <summary>
        /// Gets or sets the offset in connection B's local space from the center of mass to the anchor point which will be kept on the plane.
        /// </summary>
        public FixedV3 LocalAnchorB;

        /// <summary>
        /// Gets or sets the world space location of the line anchor attached to connection A.
        /// </summary>
        public FixedV3 PlaneAnchor
        {
            get { return ConnectionA.Position + FixedQuaternion.Transform(LocalPlaneAnchor, ConnectionA.Orientation); }
            set { LocalPlaneAnchor = FixedQuaternion.Transform(value - ConnectionA.Position, FixedQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the world space normal of the plane attached to connection A.
        /// Must be unit length.
        /// </summary>
        public FixedV3 PlaneNormal
        {
            get { return FixedQuaternion.Transform(LocalPlaneNormal, ConnectionA.Orientation); }
            set { LocalPlaneNormal = FixedQuaternion.Transform(value, FixedQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the offset in world space from the center of mass of connection B to the anchor point.
        /// </summary>
        public FixedV3 AnchorB
        {
            get { return ConnectionB.Position + FixedQuaternion.Transform(LocalAnchorB, ConnectionB.Orientation); }
            set { LocalAnchorB = FixedQuaternion.Transform(value - ConnectionB.Position, FixedQuaternion.Conjugate(ConnectionB.Orientation)); }
        }


        /// <summary>
        /// Constructs a new point on plane joint.
        /// </summary>
        /// <param name="connectionA">First bone connected by the joint.</param>
        /// <param name="connectionB">Second bone connected by the joint.</param>
        /// <param name="planeAnchor">Anchor point of the plane attached to the first bone in world space.</param>
        /// <param name="planeNormal">Normal of the plane attached to the first bone in world space. Must be unit length.</param>
        /// <param name="anchorB">Anchor point on the second bone in world space which is measured against the other connection's anchor.</param>
        public IKPointOnPlaneJoint(Bone connectionA, Bone connectionB, FixedV3 planeAnchor, FixedV3 planeNormal, FixedV3 anchorB)
            : base(connectionA, connectionB)
        {
            PlaneAnchor = planeAnchor;
            PlaneNormal = planeNormal;
            AnchorB = anchorB;
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            //Transform the anchors and offsets into world space.
            FixedV3 offsetA, offsetB, lineDirection;
            FixedQuaternion.Transform(ref LocalPlaneAnchor, ref ConnectionA.Orientation, out offsetA);
            FixedQuaternion.Transform(ref LocalPlaneNormal, ref ConnectionA.Orientation, out lineDirection);
            FixedQuaternion.Transform(ref LocalAnchorB, ref ConnectionB.Orientation, out offsetB);
            FixedV3 anchorA, anchorB;
            FixedV3.Add(ref ConnectionA.Position, ref offsetA, out anchorA);
            FixedV3.Add(ref ConnectionB.Position, ref offsetB, out anchorB);

            //Compute the distance.
            FixedV3 separation;
            FixedV3.Subtract(ref anchorB, ref anchorA, out separation);
            //This entire constraint is very similar to the IKDistanceLimit, except the current distance is along an axis.
            Fixed64 currentDistance;
            FixedV3.Dot(ref separation, ref lineDirection, out currentDistance);
            velocityBias = new FixedV3(errorCorrectionFactor * currentDistance, F64.C0, F64.C0);

            //Compute jacobians
            FixedV3 angularA, angularB;
            //We can't just use the offset to anchor for A's jacobian- the 'collision' location is way out there at anchorB!
            FixedV3 rA;
            FixedV3.Subtract(ref anchorB, ref ConnectionA.Position, out rA);
            FixedV3.Cross(ref rA, ref lineDirection, out angularA);
            //linearB = -linearA, so just swap the cross product order.
            FixedV3.Cross(ref lineDirection, ref offsetB, out angularB);

            //Put all the 1x3 jacobians into a 3x3 matrix representation.
            linearJacobianA = new BEPUMatrix3x3 { M11 = lineDirection.X, M12 = lineDirection.Y, M13 = lineDirection.Z };
            linearJacobianB = new BEPUMatrix3x3 { M11 = -lineDirection.X, M12 = -lineDirection.Y, M13 = -lineDirection.Z };
            angularJacobianA = new BEPUMatrix3x3 { M11 = angularA.X, M12 = angularA.Y, M13 = angularA.Z };
            angularJacobianB = new BEPUMatrix3x3 { M11 = angularB.X, M12 = angularB.Y, M13 = angularB.Z };

        }
    }
}

using BEPUphysics.Entities;

using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Constraints.TwoEntity.Joints
{
    /// <summary>
    /// Constrains a point on one body to be on a plane defined by another body.
    /// </summary>
    public class PointOnPlaneJoint : BEPUJoint, I1DImpulseConstraintWithError, I1DJacobianConstraint
    {
        private Fixed64 accumulatedImpulse;
        private Fixed64 biasVelocity;
        private Fixed64 error;

        private FixedV3 localPlaneAnchor;
        private FixedV3 localPlaneNormal;
        private FixedV3 localPointAnchor;

        private FixedV3 worldPlaneAnchor;
        private FixedV3 worldPlaneNormal;
        private FixedV3 worldPointAnchor;
        private Fixed64 negativeEffectiveMass;
        private FixedV3 rA;
        private FixedV3 rAcrossN;
        private FixedV3 rB;
        private FixedV3 rBcrossN;

        /// <summary>
        /// Constructs a new point on plane constraint.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the PlaneAnchor, PlaneNormal, and PointAnchor (or their entity-local versions).
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public PointOnPlaneJoint()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a new point on plane constraint.
        /// </summary>
        /// <param name="connectionA">Entity to which the constraint's plane is attached.</param>
        /// <param name="connectionB">Entity to which the constraint's point is attached.</param>
        /// <param name="planeAnchor">A point on the plane.</param>
        /// <param name="normal">Direction, attached to the first connected entity, defining the plane's normal</param>
        /// <param name="pointAnchor">The point to constrain to the plane, attached to the second connected object.</param>
        public PointOnPlaneJoint(Entity connectionA, Entity connectionB, FixedV3 planeAnchor, FixedV3 normal, FixedV3 pointAnchor)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;

            PointAnchor = pointAnchor;
            PlaneAnchor = planeAnchor;
            PlaneNormal = normal;
        }

        /// <summary>
        /// Gets or sets the plane's anchor in entity A's local space.
        /// </summary>
        public FixedV3 LocalPlaneAnchor
        {
            get { return localPlaneAnchor; }
            set
            {
                localPlaneAnchor = value;
                BEPUMatrix3x3.Transform(ref localPlaneAnchor, ref connectionA.orientationMatrix, out worldPlaneAnchor);
                FixedV3.Add(ref connectionA.position, ref worldPlaneAnchor, out worldPlaneAnchor);
            }
        }

        /// <summary>
        /// Gets or sets the plane's normal in entity A's local space.
        /// </summary>
        public FixedV3 LocalPlaneNormal
        {
            get { return localPlaneNormal; }
            set
            {
                localPlaneNormal = FixedV3.Normalize(value);
                BEPUMatrix3x3.Transform(ref localPlaneNormal, ref connectionA.orientationMatrix, out worldPlaneNormal);
            }
        }

        /// <summary>
        /// Gets or sets the point anchor in entity B's local space.
        /// </summary>
        public FixedV3 LocalPointAnchor
        {
            get { return localPointAnchor; }
            set
            {
                localPointAnchor = value;
                BEPUMatrix3x3.Transform(ref localPointAnchor, ref connectionB.orientationMatrix, out worldPointAnchor);
                FixedV3.Add(ref worldPointAnchor, ref connectionB.position, out worldPointAnchor);
            }
        }

        /// <summary>
        /// Gets the offset from A to the connection point between the entities.
        /// </summary>
        public FixedV3 OffsetA
        {
            get { return rA; }
        }

        /// <summary>
        /// Gets the offset from B to the connection point between the entities.
        /// </summary>
        public FixedV3 OffsetB
        {
            get { return rB; }
        }

        /// <summary>
        /// Gets or sets the plane anchor in world space.
        /// </summary>
        public FixedV3 PlaneAnchor
        {
            get { return worldPlaneAnchor; }
            set
            {
                worldPlaneAnchor = value;
                localPlaneAnchor = value - connectionA.position;
                BEPUMatrix3x3.TransformTranspose(ref localPlaneAnchor, ref connectionA.orientationMatrix, out localPlaneAnchor);

            }
        }

        /// <summary>
        /// Gets or sets the plane's normal in world space.
        /// </summary>
        public FixedV3 PlaneNormal
        {
            get { return worldPlaneNormal; }
            set
            {
                worldPlaneNormal = FixedV3.Normalize(value);
                BEPUMatrix3x3.TransformTranspose(ref worldPlaneNormal, ref connectionA.orientationMatrix, out localPlaneNormal);
            }
        }

        /// <summary>
        /// Gets or sets the point anchor in world space.
        /// </summary>
        public FixedV3 PointAnchor
        {
            get { return worldPointAnchor; }
            set
            {
                worldPointAnchor = value;
                localPointAnchor = value - connectionB.position;
                BEPUMatrix3x3.TransformTranspose(ref localPointAnchor, ref connectionB.orientationMatrix, out localPointAnchor);

            }
        }

        #region I1DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        public Fixed64 RelativeVelocity
        {
            get
            {
                FixedV3 dv;
                FixedV3 aVel, bVel;
                FixedV3.Cross(ref connectionA.angularVelocity, ref rA, out aVel);
                FixedV3.Add(ref aVel, ref connectionA.linearVelocity, out aVel);
                FixedV3.Cross(ref connectionB.angularVelocity, ref rB, out bVel);
                FixedV3.Add(ref bVel, ref connectionB.linearVelocity, out bVel);
                FixedV3.Subtract(ref aVel, ref bVel, out dv);
                Fixed64 velocityDifference;
                FixedV3.Dot(ref dv, ref worldPlaneNormal, out velocityDifference);
                return velocityDifference;
            }
        }


        /// <summary>
        /// Gets the total impulse applied by this constraint.
        /// </summary>
        public Fixed64 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the current constraint error.
        /// </summary>
        public Fixed64 Error
        {
            get { return error; }
        }

        #endregion

        #region I1DJacobianConstraint Members

        /// <summary>
        /// Gets the linear jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobian">Linear jacobian entry for the first connected entity.</param>
        public void GetLinearJacobianA(out FixedV3 jacobian)
        {
            jacobian = worldPlaneNormal;
        }

        /// <summary>
        /// Gets the linear jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Linear jacobian entry for the second connected entity.</param>
        public void GetLinearJacobianB(out FixedV3 jacobian)
        {
            jacobian = -worldPlaneNormal;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the first connected entity.</param>
        public void GetAngularJacobianA(out FixedV3 jacobian)
        {
            jacobian = rAcrossN;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out FixedV3 jacobian)
        {
            jacobian = -rBcrossN;
        }

        /// <summary>
        /// Gets the mass matrix of the constraint.
        /// </summary>
        /// <param name="outputMassMatrix">Constraint's mass matrix.</param>
        public void GetMassMatrix(out Fixed64 outputMassMatrix)
        {
            outputMassMatrix = -negativeEffectiveMass;
        }

        #endregion

        /// <summary>
        /// Computes one iteration of the constraint to meet the solver updateable's goal.
        /// </summary>
        /// <returns>The rough applied impulse magnitude.</returns>
        public override Fixed64 SolveIteration()
        {
            //TODO: This could technically be faster.
            //Form the jacobian explicitly.
            //Cross cross add add subtract dot
            //vs
            //dot dot dot dot and then scalar adds
            FixedV3 dv;
            FixedV3 aVel, bVel;
            FixedV3.Cross(ref connectionA.angularVelocity, ref rA, out aVel);
            FixedV3.Add(ref aVel, ref connectionA.linearVelocity, out aVel);
            FixedV3.Cross(ref connectionB.angularVelocity, ref rB, out bVel);
            FixedV3.Add(ref bVel, ref connectionB.linearVelocity, out bVel);
            FixedV3.Subtract(ref aVel, ref bVel, out dv);
            Fixed64 velocityDifference;
            FixedV3.Dot(ref dv, ref worldPlaneNormal, out velocityDifference);
            //if(velocityDifference > 0)
            //    Debug.WriteLine("Velocity difference: " + velocityDifference);
            //Debug.WriteLine("softness velocity: " + softness * accumulatedImpulse);
            Fixed64 lambda = negativeEffectiveMass * (velocityDifference + biasVelocity + softness * accumulatedImpulse);
            accumulatedImpulse += lambda;

            FixedV3 impulse;
            FixedV3 torque;
            FixedV3.Multiply(ref worldPlaneNormal, lambda, out impulse);
            if (connectionA.isDynamic)
            {
                FixedV3.Multiply(ref rAcrossN, lambda, out torque);
                connectionA.ApplyLinearImpulse(ref impulse);
                connectionA.ApplyAngularImpulse(ref torque);
            }
            if (connectionB.isDynamic)
            {
                FixedV3.Negate(ref impulse, out impulse);
                FixedV3.Multiply(ref rBcrossN, lambda, out torque);
                connectionB.ApplyLinearImpulse(ref impulse);
                connectionB.ApplyAngularImpulse(ref torque);
            }

            return lambda;
        }

        ///<summary>
        /// Performs the frame's configuration step.
        ///</summary>
        ///<param name="dt">Timestep duration.</param>
        public override void Update(Fixed64 dt)
        {
            BEPUMatrix3x3.Transform(ref localPlaneNormal, ref connectionA.orientationMatrix, out worldPlaneNormal);
            BEPUMatrix3x3.Transform(ref localPlaneAnchor, ref connectionA.orientationMatrix, out worldPlaneAnchor);
            FixedV3.Add(ref worldPlaneAnchor, ref connectionA.position, out worldPlaneAnchor);

            BEPUMatrix3x3.Transform(ref localPointAnchor, ref connectionB.orientationMatrix, out rB);
            FixedV3.Add(ref rB, ref connectionB.position, out worldPointAnchor);

            //Find rA and rB.
            //So find the closest point on the plane to worldPointAnchor.
            Fixed64 pointDistance, planeDistance;
            FixedV3.Dot(ref worldPointAnchor, ref worldPlaneNormal, out pointDistance);
            FixedV3.Dot(ref worldPlaneAnchor, ref worldPlaneNormal, out planeDistance);
            Fixed64 distanceChange = planeDistance - pointDistance;
            FixedV3 closestPointOnPlane;
            FixedV3.Multiply(ref worldPlaneNormal, distanceChange, out closestPointOnPlane);
            FixedV3.Add(ref closestPointOnPlane, ref worldPointAnchor, out closestPointOnPlane);

            FixedV3.Subtract(ref closestPointOnPlane, ref connectionA.position, out rA);

            FixedV3.Cross(ref rA, ref worldPlaneNormal, out rAcrossN);
            FixedV3.Cross(ref rB, ref worldPlaneNormal, out rBcrossN);
            FixedV3.Negate(ref rBcrossN, out rBcrossN);

            FixedV3 offset;
            FixedV3.Subtract(ref worldPointAnchor, ref closestPointOnPlane, out offset);
            FixedV3.Dot(ref offset, ref worldPlaneNormal, out error);
            Fixed64 errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReduction, out softness);
            biasVelocity = MathHelper.Clamp(-errorReduction * error, -maxCorrectiveVelocity, maxCorrectiveVelocity);

            if (connectionA.IsDynamic && connectionB.IsDynamic)
            {
                FixedV3 IrACrossN, IrBCrossN;
                BEPUMatrix3x3.Transform(ref rAcrossN, ref connectionA.inertiaTensorInverse, out IrACrossN);
                BEPUMatrix3x3.Transform(ref rBcrossN, ref connectionB.inertiaTensorInverse, out IrBCrossN);
                Fixed64 angularA, angularB;
                FixedV3.Dot(ref rAcrossN, ref IrACrossN, out angularA);
                FixedV3.Dot(ref rBcrossN, ref IrBCrossN, out angularB);
                negativeEffectiveMass = connectionA.inverseMass + connectionB.inverseMass + angularA + angularB;
                negativeEffectiveMass = -1 / (negativeEffectiveMass + softness);
            }
            else if (connectionA.IsDynamic && !connectionB.IsDynamic)
            {
                FixedV3 IrACrossN;
                BEPUMatrix3x3.Transform(ref rAcrossN, ref connectionA.inertiaTensorInverse, out IrACrossN);
                Fixed64 angularA;
                FixedV3.Dot(ref rAcrossN, ref IrACrossN, out angularA);
                negativeEffectiveMass = connectionA.inverseMass + angularA;
                negativeEffectiveMass = -1 / (negativeEffectiveMass + softness);
            }
            else if (!connectionA.IsDynamic && connectionB.IsDynamic)
            {
                FixedV3 IrBCrossN;
                BEPUMatrix3x3.Transform(ref rBcrossN, ref connectionB.inertiaTensorInverse, out IrBCrossN);
                Fixed64 angularB;
                FixedV3.Dot(ref rBcrossN, ref IrBCrossN, out angularB);
                negativeEffectiveMass = connectionB.inverseMass + angularB;
                negativeEffectiveMass = -1 / (negativeEffectiveMass + softness);
            }
            else
                negativeEffectiveMass = F64.C0;


        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //Warm Starting
            FixedV3 impulse;
            FixedV3 torque;
            FixedV3.Multiply(ref worldPlaneNormal, accumulatedImpulse, out impulse);
            if (connectionA.isDynamic)
            {
                FixedV3.Multiply(ref rAcrossN, accumulatedImpulse, out torque);
                connectionA.ApplyLinearImpulse(ref impulse);
                connectionA.ApplyAngularImpulse(ref torque);
            }
            if (connectionB.isDynamic)
            {
                FixedV3.Negate(ref impulse, out impulse);
                FixedV3.Multiply(ref rBcrossN, accumulatedImpulse, out torque);
                connectionB.ApplyLinearImpulse(ref impulse);
                connectionB.ApplyAngularImpulse(ref torque);
            }
        }
    }
}
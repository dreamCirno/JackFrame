using FixMath.NET;

namespace BEPUPhysics1int.Constraints.TwoEntity.JointLimits
{
    /// <summary>
    /// Constrains the distance along an axis between anchor points attached to two entities.
    /// </summary>
    public class LinearAxisLimit : JointLimit, I1DImpulseConstraintWithError, I1DJacobianConstraint
    {
        private Fixed64 accumulatedImpulse;
        private Fixed64 biasVelocity;
        private FixedV3 jAngularA, jAngularB;
        private FixedV3 jLinearA, jLinearB;
        private FixedV3 localAnchorA;
        private FixedV3 localAnchorB;
        private Fixed64 massMatrix;
        private Fixed64 error;
        private FixedV3 localAxis;
        private Fixed64 maximum;
        private Fixed64 minimum;
        private FixedV3 worldAxis;
        private FixedV3 rA; //Jacobian entry for entity A.
        private Fixed64 unadjustedError;
        private FixedV3 worldAnchorA;
        private FixedV3 worldAnchorB;
        private FixedV3 worldOffsetA, worldOffsetB;

        /// <summary>
        /// Constructs a constraint which tries to keep anchors on two entities within a certain distance of each other along an axis.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the AnchorA, AnchorB, and Axis (or their entity-local versions),
        /// and the Minimum and Maximum.
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public LinearAxisLimit()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a constraint which tries to keep anchors on two entities within a certain distance of each other along an axis.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        /// <param name="anchorA">World space point to attach to connection A that will be constrained.</param>
        /// <param name="anchorB">World space point to attach to connection B that will be constrained.</param>
        /// <param name="axis">Limited axis in world space to attach to connection A.</param>
        /// <param name="minimum">Minimum allowed position along the axis.</param>
        /// <param name="maximum">Maximum allowed position along the axis.</param>
        public LinearAxisLimit(Entity connectionA, Entity connectionB, FixedV3 anchorA, FixedV3 anchorB, FixedV3 axis, Fixed64 minimum, Fixed64 maximum)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;
            AnchorA = anchorA;
            AnchorB = anchorB;
            Axis = axis;
            Minimum = minimum;
            Maximum = maximum;
        }

        /// <summary>
        /// Gets or sets the anchor point attached to entity A in world space.
        /// </summary>
        public FixedV3 AnchorA
        {
            get { return worldAnchorA; }
            set
            {
                worldAnchorA = value;
                worldOffsetA = worldAnchorA - connectionA.position;
                BEPUMatrix3x3.TransformTranspose(ref worldOffsetA, ref connectionA.orientationMatrix, out localAnchorA);
            }
        }

        /// <summary>
        /// Gets or sets the anchor point attached to entity A in world space.
        /// </summary>
        public FixedV3 AnchorB
        {
            get { return worldAnchorB; }
            set
            {
                worldAnchorB = value;
                worldOffsetB = worldAnchorB - connectionB.position;
                BEPUMatrix3x3.TransformTranspose(ref worldOffsetB, ref connectionB.orientationMatrix, out localAnchorB);
            }
        }

        /// <summary>
        /// Gets or sets the limited axis in world space.
        /// </summary>
        public FixedV3 Axis
        {
            get { return worldAxis; }
            set
            {
                worldAxis = FixedV3.Normalize(value);
                BEPUMatrix3x3.TransformTranspose(ref worldAxis, ref connectionA.orientationMatrix, out localAxis);
            }
        }

        /// <summary>
        /// Gets or sets the limited axis in the local space of connection A.
        /// </summary>
        public FixedV3 LocalAxis
        {
            get { return localAxis; }
            set
            {
                localAxis = FixedV3.Normalize(value);
                BEPUMatrix3x3.Transform(ref localAxis, ref connectionA.orientationMatrix, out worldAxis);
            }
        }

        /// <summary>
        /// Gets or sets the offset from the first entity's center of mass to the anchor point in its local space.
        /// </summary>
        public FixedV3 LocalOffsetA
        {
            get { return localAnchorA; }
            set
            {
                localAnchorA = value;
                BEPUMatrix3x3.Transform(ref localAnchorA, ref connectionA.orientationMatrix, out worldOffsetA);
                worldAnchorA = connectionA.position + worldOffsetA;
            }
        }

        /// <summary>
        /// Gets or sets the offset from the second entity's center of mass to the anchor point in its local space.
        /// </summary>
        public FixedV3 LocalOffsetB
        {
            get { return localAnchorB; }
            set
            {
                localAnchorB = value;
                BEPUMatrix3x3.Transform(ref localAnchorB, ref connectionB.orientationMatrix, out worldOffsetB);
                worldAnchorB = connectionB.position + worldOffsetB;
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed distance along the axis.
        /// </summary>
        public Fixed64 Maximum
        {
            get { return maximum; }
            set
            {
                maximum = value;
                minimum = MathHelper.Min(minimum, maximum);
            }
        }

        /// <summary>
        /// Gets or sets the minimum allowed distance along the axis.
        /// </summary>
        public Fixed64 Minimum
        {
            get { return minimum; }
            set
            {
                minimum = value;
                maximum = MathHelper.Max(minimum, maximum);
            }
        }

        /// <summary>
        /// Gets or sets the offset from the first entity's center of mass to the anchor point in world space.
        /// </summary>
        public FixedV3 OffsetA
        {
            get { return worldOffsetA; }
            set
            {
                worldOffsetA = value;
                worldAnchorA = connectionA.position + worldOffsetA;
                BEPUMatrix3x3.TransformTranspose(ref worldOffsetA, ref connectionA.orientationMatrix, out localAnchorA);
            }
        }

        /// <summary>
        /// Gets or sets the offset from the second entity's center of mass to the anchor point in world space.
        /// </summary>
        public FixedV3 OffsetB
        {
            get { return worldOffsetB; }
            set
            {
                worldOffsetB = value;
                worldAnchorB = connectionB.position + worldOffsetB;
                BEPUMatrix3x3.TransformTranspose(ref worldOffsetB, ref connectionB.orientationMatrix, out localAnchorB);
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
                if (isLimitActive)
                {
                    Fixed64 lambda, dot;
                    FixedV3.Dot(ref jLinearA, ref connectionA.linearVelocity, out lambda);
                    FixedV3.Dot(ref jAngularA, ref connectionA.angularVelocity, out dot);
                    lambda += dot;
                    FixedV3.Dot(ref jLinearB, ref connectionB.linearVelocity, out dot);
                    lambda += dot;
                    FixedV3.Dot(ref jAngularB, ref connectionB.angularVelocity, out dot);
                    lambda += dot;
                    return lambda;
                }
                return F64.C0;
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

        //Jacobians

        #region I1DJacobianConstraint Members

        /// <summary>
        /// Gets the linear jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobian">Linear jacobian entry for the first connected entity.</param>
        public void GetLinearJacobianA(out FixedV3 jacobian)
        {
            jacobian = jLinearA;
        }

        /// <summary>
        /// Gets the linear jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Linear jacobian entry for the second connected entity.</param>
        public void GetLinearJacobianB(out FixedV3 jacobian)
        {
            jacobian = jLinearB;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the first connected entity.</param>
        public void GetAngularJacobianA(out FixedV3 jacobian)
        {
            jacobian = jAngularA;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out FixedV3 jacobian)
        {
            jacobian = jAngularB;
        }

        /// <summary>
        /// Gets the mass matrix of the constraint.
        /// </summary>
        /// <param name="outputMassMatrix">Constraint's mass matrix.</param>
        public void GetMassMatrix(out Fixed64 outputMassMatrix)
        {
            outputMassMatrix = massMatrix;
        }

        #endregion

        /// <summary>
        /// Computes one iteration of the constraint to meet the solver updateable's goal.
        /// </summary>
        /// <returns>The rough applied impulse magnitude.</returns>
        public override Fixed64 SolveIteration()
        {
            //Compute the current relative velocity.
            Fixed64 lambda, dot;
            FixedV3.Dot(ref jLinearA, ref connectionA.linearVelocity, out lambda);
            FixedV3.Dot(ref jAngularA, ref connectionA.angularVelocity, out dot);
            lambda += dot;
            FixedV3.Dot(ref jLinearB, ref connectionB.linearVelocity, out dot);
            lambda += dot;
            FixedV3.Dot(ref jAngularB, ref connectionB.angularVelocity, out dot);
            lambda += dot;

            //Add in the constraint space bias velocity
            lambda = -lambda + biasVelocity - softness * accumulatedImpulse;

            //Transform to an impulse
            lambda *= massMatrix;

            //Clamp accumulated impulse (can't go negative)
            Fixed64 previousAccumulatedImpulse = accumulatedImpulse;
            if (unadjustedError < F64.C0)
                accumulatedImpulse = MathHelper.Min(accumulatedImpulse + lambda, F64.C0);
            else
                accumulatedImpulse = MathHelper.Max(accumulatedImpulse + lambda, F64.C0);
            lambda = accumulatedImpulse - previousAccumulatedImpulse;

            //Apply the impulse
            FixedV3 impulse;
            if (connectionA.isDynamic)
            {
                FixedV3.Multiply(ref jLinearA, lambda, out impulse);
                connectionA.ApplyLinearImpulse(ref impulse);
                FixedV3.Multiply(ref jAngularA, lambda, out impulse);
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                FixedV3.Multiply(ref jLinearB, lambda, out impulse);
                connectionB.ApplyLinearImpulse(ref impulse);
                FixedV3.Multiply(ref jAngularB, lambda, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }

            return (Fixed64.Abs(lambda));
        }

        ///<summary>
        /// Performs the frame's configuration step.
        ///</summary>
        ///<param name="dt">Timestep duration.</param>
        public override void Update(Fixed64 dt)
        {
            //Compute the 'pre'-jacobians
            BEPUMatrix3x3.Transform(ref localAnchorA, ref connectionA.orientationMatrix, out worldOffsetA);
            BEPUMatrix3x3.Transform(ref localAnchorB, ref connectionB.orientationMatrix, out worldOffsetB);
            FixedV3.Add(ref worldOffsetA, ref connectionA.position, out worldAnchorA);
            FixedV3.Add(ref worldOffsetB, ref connectionB.position, out worldAnchorB);
            FixedV3.Subtract(ref worldAnchorB, ref connectionA.position, out rA);
            BEPUMatrix3x3.Transform(ref localAxis, ref connectionA.orientationMatrix, out worldAxis);

            //Compute error
#if !WINDOWS
            FixedV3 separation = new FixedV3();
#else
            Vector3 separation;
#endif
            separation.X = worldAnchorB.X - worldAnchorA.X;
            separation.Y = worldAnchorB.Y - worldAnchorA.Y;
            separation.Z = worldAnchorB.Z - worldAnchorA.Z;

            FixedV3.Dot(ref separation, ref worldAxis, out unadjustedError);

            //Compute error
            if (unadjustedError < minimum)
                unadjustedError = minimum - unadjustedError;
            else if (unadjustedError > maximum)
                unadjustedError = maximum - unadjustedError;
            else
            {
                unadjustedError = F64.C0;
                isActiveInSolver = false;
                accumulatedImpulse = F64.C0;
                isLimitActive = false;
                return;
            }
            isLimitActive = true;

            unadjustedError = -unadjustedError;
            //Adjust Error
            if (unadjustedError > F64.C0)
                error = MathHelper.Max(F64.C0, unadjustedError - margin);
            else if (unadjustedError < F64.C0)
                error = MathHelper.Min(F64.C0, unadjustedError + margin);

            //Compute jacobians
            jLinearA = worldAxis;
            jLinearB.X = -jLinearA.X;
            jLinearB.Y = -jLinearA.Y;
            jLinearB.Z = -jLinearA.Z;
            FixedV3.Cross(ref rA, ref jLinearA, out jAngularA);
            FixedV3.Cross(ref worldOffsetB, ref jLinearB, out jAngularB);

            //Compute bias
            Fixed64 errorReductionParameter;
            springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReductionParameter, out softness);

            biasVelocity = MathHelper.Clamp(errorReductionParameter * error, -maxCorrectiveVelocity, maxCorrectiveVelocity);
            if (bounciness > F64.C0)
            {
                //Compute currently relative velocity for bounciness.
                Fixed64 relativeVelocity, dot;
                FixedV3.Dot(ref jLinearA, ref connectionA.linearVelocity, out relativeVelocity);
                FixedV3.Dot(ref jAngularA, ref connectionA.angularVelocity, out dot);
                relativeVelocity += dot;
                FixedV3.Dot(ref jLinearB, ref connectionB.linearVelocity, out dot);
                relativeVelocity += dot;
                FixedV3.Dot(ref jAngularB, ref connectionB.angularVelocity, out dot);
                relativeVelocity += dot;
                if (unadjustedError > F64.C0 && -relativeVelocity > bounceVelocityThreshold)
                    biasVelocity = MathHelper.Max(biasVelocity, ComputeBounceVelocity(-relativeVelocity));
                else if (unadjustedError < F64.C0 && relativeVelocity > bounceVelocityThreshold)
                    biasVelocity = MathHelper.Min(biasVelocity, -ComputeBounceVelocity(relativeVelocity));
            }


            //compute mass matrix
            Fixed64 entryA, entryB;
            FixedV3 intermediate;
            if (connectionA.isDynamic)
            {
                BEPUMatrix3x3.Transform(ref jAngularA, ref connectionA.inertiaTensorInverse, out intermediate);
                FixedV3.Dot(ref intermediate, ref jAngularA, out entryA);
                entryA += connectionA.inverseMass;
            }
            else
                entryA = F64.C0;
            if (connectionB.isDynamic)
            {
                BEPUMatrix3x3.Transform(ref jAngularB, ref connectionB.inertiaTensorInverse, out intermediate);
                FixedV3.Dot(ref intermediate, ref jAngularB, out entryB);
                entryB += connectionB.inverseMass;
            }
            else
                entryB = F64.C0;
            massMatrix = F64.C1 / (entryA + entryB + softness);


            
        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //Warm starting
            FixedV3 impulse;
            if (connectionA.isDynamic)
            {
                FixedV3.Multiply(ref jLinearA, accumulatedImpulse, out impulse);
                connectionA.ApplyLinearImpulse(ref impulse);
                FixedV3.Multiply(ref jAngularA, accumulatedImpulse, out impulse);
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                FixedV3.Multiply(ref jLinearB, accumulatedImpulse, out impulse);
                connectionB.ApplyLinearImpulse(ref impulse);
                FixedV3.Multiply(ref jAngularB, accumulatedImpulse, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }
        } 
    }
}
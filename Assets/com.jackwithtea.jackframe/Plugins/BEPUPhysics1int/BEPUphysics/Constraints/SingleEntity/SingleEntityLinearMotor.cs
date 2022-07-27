using BEPUPhysics1int.Constraints.TwoEntity.Motors;
using FixMath.NET;

namespace BEPUPhysics1int.Constraints.SingleEntity
{
    /// <summary>
    /// Constraint which tries to push an entity to a desired location.
    /// </summary>
    public class SingleEntityLinearMotor : SingleEntityConstraint, I3DImpulseConstraintWithError
    {
        private readonly MotorSettings3D settings;

        /// <summary>
        /// Sum of forces applied to the constraint in the past.
        /// </summary>
        private FixedV3 accumulatedImpulse = FixedV3.Zero;

        private FixedV3 biasVelocity;
        private BEPUMatrix3x3 effectiveMassMatrix;

        /// <summary>
        /// Maximum impulse that can be applied in a single frame.
        /// </summary>
        private Fixed64 maxForceDt;

        /// <summary>
        /// Maximum impulse that can be applied in a single frame, squared.
        /// This is computed in the prestep to avoid doing extra multiplies in the more-often called applyImpulse method.
        /// </summary>
        private Fixed64 maxForceDtSquared;

        private FixedV3 error;

        private FixedV3 localPoint;

        private FixedV3 worldPoint;

        private FixedV3 r;
        private Fixed64 usedSoftness;

        /// <summary>
        /// Gets or sets the entity affected by the constraint.
        /// </summary>
        public override Entity Entity
        {
            get
            {
                return base.Entity;
            }
            set
            {
                if (Entity != value)
                    accumulatedImpulse = new FixedV3();
                base.Entity = value;
            }
        }


        /// <summary>
        /// Constructs a new single body linear motor.  This motor will try to move a single entity to a goal velocity or to a goal position.
        /// </summary>
        /// <param name="entity">Entity to affect.</param>
        /// <param name="point">Point in world space attached to the entity that will be motorized.</param>
        public SingleEntityLinearMotor(Entity entity, FixedV3 point)
        {
            Entity = entity;
            Point = point;

            settings = new MotorSettings3D(this) {servo = {goal = point}};
            //Not really necessary, just helps prevent 'snapping'.
        }


        /// <summary>
        /// Constructs a new single body linear motor.  This motor will try to move a single entity to a goal velocity or to a goal position.
        /// This constructor will start the motor with isActive = false.
        /// </summary>
        public SingleEntityLinearMotor()
        {
            settings = new MotorSettings3D(this);
            IsActive = false;
        }

        /// <summary>
        /// Point attached to the entity in its local space that is motorized.
        /// </summary>
        public FixedV3 LocalPoint
        {
            get { return localPoint; }
            set
            {
                localPoint = value;
                BEPUMatrix3x3.Transform(ref localPoint, ref entity.orientationMatrix, out worldPoint);
                FixedV3.Add(ref worldPoint, ref entity.position, out worldPoint);
            }
        }

        /// <summary>
        /// Point attached to the entity in world space that is motorized.
        /// </summary>
        public FixedV3 Point
        {
            get { return worldPoint; }
            set
            {
                worldPoint = value;
                FixedV3.Subtract(ref worldPoint, ref entity.position, out localPoint);
                BEPUMatrix3x3.TransformTranspose(ref localPoint, ref entity.orientationMatrix, out localPoint);
            }
        }

        /// <summary>
        /// Gets the motor's velocity and servo settings.
        /// </summary>
        public MotorSettings3D Settings
        {
            get { return settings; }
        }

        #region I3DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        public FixedV3 RelativeVelocity
        {
            get
            {
                FixedV3 lambda;
                FixedV3.Cross(ref r, ref entity.angularVelocity, out lambda);
                FixedV3.Subtract(ref lambda, ref entity.linearVelocity, out lambda);
                return lambda;
            }
        }

        /// <summary>
        /// Gets the total impulse applied by this constraint.
        /// </summary>
        public FixedV3 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the current constraint error.
        /// If the motor is in velocity only mode, error is zero.
        /// </summary>
        public FixedV3 Error
        {
            get { return error; }
        }

        #endregion

        /// <summary>
        /// Computes one iteration of the constraint to meet the solver updateable's goal.
        /// </summary>
        /// <returns>The rough applied impulse magnitude.</returns>
        public override Fixed64 SolveIteration()
        {
            //Compute relative velocity
            FixedV3 lambda;
            FixedV3.Cross(ref r, ref entity.angularVelocity, out lambda);
            FixedV3.Subtract(ref lambda, ref entity.linearVelocity, out lambda);

            //Add in bias velocity
            FixedV3.Add(ref biasVelocity, ref lambda, out lambda);

            //Add in softness
            FixedV3 softnessVelocity;
            FixedV3.Multiply(ref accumulatedImpulse, usedSoftness, out softnessVelocity);
            FixedV3.Subtract(ref lambda, ref softnessVelocity, out lambda);

            //In terms of an impulse (an instantaneous change in momentum), what is it?
            BEPUMatrix3x3.Transform(ref lambda, ref effectiveMassMatrix, out lambda);

            //Sum the impulse.
            FixedV3 previousAccumulatedImpulse = accumulatedImpulse;
            accumulatedImpulse += lambda;

            //If the impulse it takes to get to the goal is too high for the motor to handle, scale it back.
            Fixed64 sumImpulseLengthSquared = accumulatedImpulse.LengthSquared();
            if (sumImpulseLengthSquared > maxForceDtSquared)
            {
                //max / impulse gives some value 0 < x < 1.  Basically, normalize the vector (divide by the length) and scale by the maximum.
                accumulatedImpulse *= maxForceDt / Fixed64.Sqrt(sumImpulseLengthSquared);

                //Since the limit was exceeded by this corrective impulse, limit it so that the accumulated impulse remains constrained.
                lambda = accumulatedImpulse - previousAccumulatedImpulse;
            }


            entity.ApplyLinearImpulse(ref lambda);
            FixedV3 taImpulse;
            FixedV3.Cross(ref r, ref lambda, out taImpulse);
            entity.ApplyAngularImpulse(ref taImpulse);

            return (Fixed64.Abs(lambda.X) + Fixed64.Abs(lambda.Y) + Fixed64.Abs(lambda.Z));
        }

        ///<summary>
        /// Performs the frame's configuration step.
        ///</summary>
        ///<param name="dt">Timestep duration.</param>
        public override void Update(Fixed64 dt)
        {
            //Transform point into world space.
            BEPUMatrix3x3.Transform(ref localPoint, ref entity.orientationMatrix, out r);
            FixedV3.Add(ref r, ref entity.position, out worldPoint);

            Fixed64 updateRate = F64.C1 / dt;
            if (settings.mode == MotorMode.Servomechanism)
            {
                FixedV3.Subtract(ref settings.servo.goal, ref worldPoint, out error);
                Fixed64 separationDistance = error.Length();
                if (separationDistance > Toolbox.BigEpsilon)
                {
                    Fixed64 errorReduction;
                    settings.servo.springSettings.ComputeErrorReductionAndSoftness(dt, updateRate, out errorReduction, out usedSoftness);

                    //The rate of correction can be based on a constant correction velocity as well as a 'spring like' correction velocity.
                    //The constant correction velocity could overshoot the destination, so clamp it.
                    Fixed64 correctionSpeed = MathHelper.Min(settings.servo.baseCorrectiveSpeed, separationDistance * updateRate) +
                                            separationDistance * errorReduction;

                    FixedV3.Multiply(ref error, correctionSpeed / separationDistance, out biasVelocity);
                    //Ensure that the corrective velocity doesn't exceed the max.
                    Fixed64 length = biasVelocity.LengthSquared();
                    if (length > settings.servo.maxCorrectiveVelocitySquared)
                    {
                        Fixed64 multiplier = settings.servo.maxCorrectiveVelocity / Fixed64.Sqrt(length);
                        biasVelocity.X *= multiplier;
                        biasVelocity.Y *= multiplier;
                        biasVelocity.Z *= multiplier;
                    }
                }
                else
                {
                    //Wouldn't want to use a bias from an earlier frame.
                    biasVelocity = new FixedV3();
                }
            }
            else
            {
                usedSoftness = settings.velocityMotor.softness * updateRate;
                biasVelocity = settings.velocityMotor.goalVelocity;
                error = FixedV3.Zero;
            }

            //Compute the maximum force that can be applied this frame.
            ComputeMaxForces(settings.maximumForce, dt);

            //COMPUTE EFFECTIVE MASS MATRIX
            //Transforms a change in velocity to a change in momentum when multiplied.
            BEPUMatrix3x3 linearComponent;
            BEPUMatrix3x3.CreateScale(entity.inverseMass, out linearComponent);
            BEPUMatrix3x3 rACrossProduct;
            BEPUMatrix3x3.CreateCrossProduct(ref r, out rACrossProduct);
            BEPUMatrix3x3 angularComponentA;
            BEPUMatrix3x3.Multiply(ref rACrossProduct, ref entity.inertiaTensorInverse, out angularComponentA);
            BEPUMatrix3x3.Multiply(ref angularComponentA, ref rACrossProduct, out angularComponentA);
            BEPUMatrix3x3.Subtract(ref linearComponent, ref angularComponentA, out effectiveMassMatrix);

            effectiveMassMatrix.M11 += usedSoftness;
            effectiveMassMatrix.M22 += usedSoftness;
            effectiveMassMatrix.M33 += usedSoftness;

            BEPUMatrix3x3.Invert(ref effectiveMassMatrix, out effectiveMassMatrix);

        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //"Warm start" the constraint by applying a first guess of the solution should be.
            entity.ApplyLinearImpulse(ref accumulatedImpulse);
            FixedV3 taImpulse;
            FixedV3.Cross(ref r, ref accumulatedImpulse, out taImpulse);
            entity.ApplyAngularImpulse(ref taImpulse);
        }

        /// <summary>
        /// Computes the maxForceDt and maxForceDtSquared fields.
        /// </summary>
        private void ComputeMaxForces(Fixed64 maxForce, Fixed64 dt)
        {
            //Determine maximum force
            if (maxForce < Fixed64.MaxValue)
            {
                maxForceDt = maxForce * dt;
                maxForceDtSquared = maxForceDt * maxForceDt;
            }
            else
            {
                maxForceDt = Fixed64.MaxValue;
                maxForceDtSquared = Fixed64.MaxValue;
            }
        }
    }
}
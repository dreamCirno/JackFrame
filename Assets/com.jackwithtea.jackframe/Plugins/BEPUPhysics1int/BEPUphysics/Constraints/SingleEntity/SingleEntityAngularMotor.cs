﻿using BEPUPhysics1int.Constraints.TwoEntity.Motors;
using FixMath.NET;

namespace BEPUPhysics1int.Constraints.SingleEntity
{
    /// <summary>
    /// Constraint which attempts to restrict the relative angular velocity of two entities to some value.
    /// Can use a target relative orientation to apply additional force.
    /// </summary>
    public class SingleEntityAngularMotor : SingleEntityConstraint, I3DImpulseConstraintWithError
    {
        private readonly JointBasis3D basis = new JointBasis3D();

        private readonly MotorSettingsOrientation settings;
        private FixedV3 accumulatedImpulse;


        private Fixed64 angle;
        private FixedV3 axis;

        private FixedV3 biasVelocity;
        private BEPUMatrix3x3 effectiveMassMatrix;

        private Fixed64 maxForceDt;
        private Fixed64 maxForceDtSquared;
        private Fixed64 usedSoftness;

        /// <summary>
        /// Constructs a new constraint which attempts to restrict the angular velocity or orientation to a goal.
        /// </summary>
        /// <param name="entity">Affected entity.</param>
        public SingleEntityAngularMotor(Entity entity)
        {
            Entity = entity;

            settings = new MotorSettingsOrientation(this) {servo = {goal = base.entity.orientation}};
            //Since no target relative orientation was specified, just use the current relative orientation.  Prevents any nasty start-of-sim 'snapping.'

            //mySettings.myServo.springSettings.stiffnessConstant *= .5f;
        }

        /// <summary>
        /// Constructs a new constraint which attempts to restrict the angular velocity or orientation to a goal.
        /// This constructor will make the angular motor start with isActive set to false.
        /// </summary>
        public SingleEntityAngularMotor()
        {
            settings = new MotorSettingsOrientation(this);
            IsActive = false;
        }

        /// <summary>
        /// Gets the basis attached to the entity.
        /// The target velocity/orientation of this motor is transformed by the basis.
        /// </summary>
        public JointBasis3D Basis
        {
            get { return basis; }
        }

        /// <summary>
        /// Gets the motor's velocity and servo settings.
        /// </summary>
        public MotorSettingsOrientation Settings
        {
            get { return settings; }
        }

        #region I3DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity with respect to the constraint.
        /// For single entity constraints, this is pretty straightforward.  It is taken directly from the 
        /// entity.
        /// </summary>
        public FixedV3 RelativeVelocity
        {
            get { return -Entity.AngularVelocity; }
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
            get { return axis * angle; }
        }

        #endregion

        /// <summary>
        /// Applies the corrective impulses required by the constraint.
        /// </summary>
        public override Fixed64 SolveIteration()
        {
#if !WINDOWS
            FixedV3 lambda = new FixedV3();
#else
            Vector3 lambda;
#endif
            FixedV3 aVel = entity.angularVelocity;
            lambda.X = -aVel.X + biasVelocity.X - usedSoftness * accumulatedImpulse.X;
            lambda.Y = -aVel.Y + biasVelocity.Y - usedSoftness * accumulatedImpulse.Y;
            lambda.Z = -aVel.Z + biasVelocity.Z - usedSoftness * accumulatedImpulse.Z;

            BEPUMatrix3x3.Transform(ref lambda, ref effectiveMassMatrix, out lambda);

            FixedV3 previousAccumulatedImpulse = accumulatedImpulse;
            accumulatedImpulse.X += lambda.X;
            accumulatedImpulse.Y += lambda.Y;
            accumulatedImpulse.Z += lambda.Z;
            Fixed64 sumLengthSquared = accumulatedImpulse.LengthSquared();

            if (sumLengthSquared > maxForceDtSquared)
            {
                //max / impulse gives some value 0 < x < 1.  Basically, normalize the vector (divide by the length) and scale by the maximum.
                Fixed64 multiplier = maxForceDt / Fixed64.Sqrt(sumLengthSquared);
                accumulatedImpulse.X *= multiplier;
                accumulatedImpulse.Y *= multiplier;
                accumulatedImpulse.Z *= multiplier;

                //Since the limit was exceeded by this corrective impulse, limit it so that the accumulated impulse remains constrained.
                lambda.X = accumulatedImpulse.X - previousAccumulatedImpulse.X;
                lambda.Y = accumulatedImpulse.Y - previousAccumulatedImpulse.Y;
                lambda.Z = accumulatedImpulse.Z - previousAccumulatedImpulse.Z;
            }


            entity.ApplyAngularImpulse(ref lambda);


            return Fixed64.Abs(lambda.X) + Fixed64.Abs(lambda.Y) + Fixed64.Abs(lambda.Z);
        }

        /// <summary>
        /// Initializes the constraint for the current frame.
        /// </summary>
        /// <param name="dt">Time between frames.</param>
        public override void Update(Fixed64 dt)
        {
            basis.rotationMatrix = entity.orientationMatrix;
            basis.ComputeWorldSpaceAxes();

            Fixed64 updateRate = F64.C1 / dt;
            if (settings.mode == MotorMode.Servomechanism) //Only need to do the bulk of this work if it's a servo.
            {
                FixedQuaternion currentRelativeOrientation;
                var worldTransform = basis.WorldTransform;
                FixedQuaternion.CreateFromRotationMatrix(ref worldTransform, out currentRelativeOrientation);


                //Compute the relative orientation R' between R and the target relative orientation.
                FixedQuaternion errorOrientation;
                FixedQuaternion.Conjugate(ref currentRelativeOrientation, out errorOrientation);
                FixedQuaternion.Multiply(ref settings.servo.goal, ref errorOrientation, out errorOrientation);


                Fixed64 errorReduction;
                settings.servo.springSettings.ComputeErrorReductionAndSoftness(dt, updateRate, out errorReduction, out usedSoftness);

                //Turn this into an axis-angle representation.
                FixedQuaternion.GetAxisAngleFromQuaternion(ref errorOrientation, out axis, out angle);

                //Scale the axis by the desired velocity if the angle is sufficiently large (epsilon).
                if (angle > Toolbox.BigEpsilon)
                {
                    Fixed64 velocity = MathHelper.Min(settings.servo.baseCorrectiveSpeed, angle * updateRate) + angle * errorReduction;

                    biasVelocity.X = axis.X * velocity;
                    biasVelocity.Y = axis.Y * velocity;
                    biasVelocity.Z = axis.Z * velocity;


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
                    //Wouldn't want an old frame's bias velocity to sneak in.
                    biasVelocity = new FixedV3();
                }
            }
            else
            {
                usedSoftness = settings.velocityMotor.softness * updateRate;
                angle = F64.C0; //Zero out the error;
                BEPUMatrix3x3 transform = basis.WorldTransform;
                BEPUMatrix3x3.Transform(ref settings.velocityMotor.goalVelocity, ref transform, out biasVelocity);
            }

            //Compute effective mass
            effectiveMassMatrix = entity.inertiaTensorInverse;
            effectiveMassMatrix.M11 += usedSoftness;
            effectiveMassMatrix.M22 += usedSoftness;
            effectiveMassMatrix.M33 += usedSoftness;
            BEPUMatrix3x3.Invert(ref effectiveMassMatrix, out effectiveMassMatrix);

            //Update the maximum force
            ComputeMaxForces(settings.maximumForce, dt);


            
        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //Apply accumulated impulse
            entity.ApplyAngularImpulse(ref accumulatedImpulse);
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
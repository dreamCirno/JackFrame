﻿using FixMath.NET;

namespace BEPUPhysics1int.Constraints.TwoEntity.Motors
{
    /// <summary>
    /// Attempts to achieve some defined relative twist angle between the entities.
    /// </summary>
    public class TwistMotor : Motor, I1DImpulseConstraintWithError, I1DJacobianConstraint
    {
        private readonly JointBasis3D basisA = new JointBasis3D();
        private readonly JointBasis2D basisB = new JointBasis2D();
        private readonly MotorSettings1D settings;


        private Fixed64 accumulatedImpulse;

        /// <summary>
        /// Velocity needed to get closer to the goal.
        /// </summary>
        protected Fixed64 biasVelocity;

        private FixedV3 jacobianA, jacobianB;
        private Fixed64 error;
        private Fixed64 velocityToImpulse;


        /// <summary>
        /// Constructs a new constraint which prevents the connected entities from twisting relative to each other.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the BasisA and BasisB.
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public TwistMotor()
        {
            IsActive = false;
            settings = new MotorSettings1D(this);
        }

        /// <summary>
        /// Constructs a new constraint which prevents the connected entities from twisting relative to each other.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        /// <param name="axisA">Twist axis attached to the first connected entity.</param>
        /// <param name="axisB">Twist axis attached to the second connected entity.</param>
        public TwistMotor(Entity connectionA, Entity connectionB, FixedV3 axisA, FixedV3 axisB)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;
            SetupJointTransforms(axisA, axisB);

            settings = new MotorSettings1D(this);
        }

        /// <summary>
        /// Gets the basis attached to entity A.
        /// The primary axis represents the twist axis attached to entity A.
        /// The x axis and y axis represent a plane against which entity B's attached x axis is projected to determine the twist angle.
        /// </summary>
        public JointBasis3D BasisA
        {
            get { return basisA; }
        }


        /// <summary>
        /// Gets the basis attached to entity B.
        /// The primary axis represents the twist axis attached to entity B.
        /// The x axis is projected onto the plane defined by localTransformA's x and y axes
        /// to get the twist angle.
        /// </summary>
        public JointBasis2D BasisB
        {
            get { return basisB; }
        }

        /// <summary>
        /// Gets the motor's velocity and servo settings.
        /// </summary>
        public MotorSettings1D Settings
        {
            get { return settings; }
        }

        #region I1DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        public Fixed64 RelativeVelocity
        {
            get
            {
                Fixed64 velocityA, velocityB;
                FixedV3.Dot(ref connectionA.angularVelocity, ref jacobianA, out velocityA);
                FixedV3.Dot(ref connectionB.angularVelocity, ref jacobianB, out velocityB);
                return velocityA + velocityB;
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
        /// If the motor is in velocity only mode, the error will be zero.
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
            jacobian = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the linear jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Linear jacobian entry for the second connected entity.</param>
        public void GetLinearJacobianB(out FixedV3 jacobian)
        {
            jacobian = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the first connected entity.</param>
        public void GetAngularJacobianA(out FixedV3 jacobian)
        {
            jacobian = jacobianA;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out FixedV3 jacobian)
        {
            jacobian = jacobianB;
        }

        /// <summary>
        /// Gets the mass matrix of the constraint.
        /// </summary>
        /// <param name="outputMassMatrix">Constraint's mass matrix.</param>
        public void GetMassMatrix(out Fixed64 outputMassMatrix)
        {
            outputMassMatrix = velocityToImpulse;
        }

        #endregion

        /// <summary>
        /// Sets up the joint transforms by automatically creating perpendicular vectors to complete the bases.
        /// </summary>
        /// <param name="worldTwistAxisA">Twist axis in world space to attach to entity A.</param>
        /// <param name="worldTwistAxisB">Twist axis in world space to attach to entity B.</param>
        public void SetupJointTransforms(FixedV3 worldTwistAxisA, FixedV3 worldTwistAxisB)
        {
            worldTwistAxisA.Normalize();
            worldTwistAxisB.Normalize();

            FixedV3 worldXAxis;
            FixedV3.Cross(ref worldTwistAxisA, ref Toolbox.UpVector, out worldXAxis);
            Fixed64 length = worldXAxis.LengthSquared();
            if (length < Toolbox.Epsilon)
            {
                FixedV3.Cross(ref worldTwistAxisA, ref Toolbox.RightVector, out worldXAxis);
            }
            worldXAxis.Normalize();

            //Complete A's basis.
            FixedV3 worldYAxis;
            FixedV3.Cross(ref worldTwistAxisA, ref worldXAxis, out worldYAxis);

            basisA.rotationMatrix = connectionA.orientationMatrix;
            basisA.SetWorldAxes(worldTwistAxisA, worldXAxis, worldYAxis);

            //Rotate the axis to B since it could be arbitrarily rotated.
            FixedQuaternion rotation;
            FixedQuaternion.GetQuaternionBetweenNormalizedVectors(ref worldTwistAxisA, ref worldTwistAxisB, out rotation);
            FixedQuaternion.Transform(ref worldXAxis, ref rotation, out worldXAxis);

            basisB.rotationMatrix = connectionB.orientationMatrix;
            basisB.SetWorldAxes(worldTwistAxisB, worldXAxis);
        }

        /// <summary>
        /// Solves for velocity.
        /// </summary>
        public override Fixed64 SolveIteration()
        {
            Fixed64 velocityA, velocityB;
            //Find the velocity contribution from each connection
            FixedV3.Dot(ref connectionA.angularVelocity, ref jacobianA, out velocityA);
            FixedV3.Dot(ref connectionB.angularVelocity, ref jacobianB, out velocityB);
            //Add in the constraint space bias velocity
            Fixed64 lambda = -(velocityA + velocityB) + biasVelocity - usedSoftness * accumulatedImpulse;

            //Transform to an impulse
            lambda *= velocityToImpulse;

            //Accumulate the impulse
            Fixed64 previousAccumulatedImpulse = accumulatedImpulse;
            accumulatedImpulse = MathHelper.Clamp(accumulatedImpulse + lambda, -maxForceDt, maxForceDt);
            lambda = accumulatedImpulse - previousAccumulatedImpulse;

            //Apply the impulse
            FixedV3 impulse;
            if (connectionA.isDynamic)
            {
                FixedV3.Multiply(ref jacobianA, lambda, out impulse);
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                FixedV3.Multiply(ref jacobianB, lambda, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }

            return Fixed64.Abs(lambda);
        }

        /// <summary>
        /// Do any necessary computations to prepare the constraint for this frame.
        /// </summary>
        /// <param name="dt">Simulation step length.</param>
        public override void Update(Fixed64 dt)
        {
            basisA.rotationMatrix = connectionA.orientationMatrix;
            basisB.rotationMatrix = connectionB.orientationMatrix;
            basisA.ComputeWorldSpaceAxes();
            basisB.ComputeWorldSpaceAxes();

            if (settings.mode == MotorMode.Servomechanism)
            {
                FixedQuaternion rotation;
                FixedQuaternion.GetQuaternionBetweenNormalizedVectors(ref basisB.primaryAxis, ref basisA.primaryAxis, out rotation);

                //Transform b's 'Y' axis so that it is perpendicular with a's 'X' axis for measurement.
                FixedV3 twistMeasureAxis;
                FixedQuaternion.Transform(ref basisB.xAxis, ref rotation, out twistMeasureAxis);


                //By dotting the measurement vector with a 2d plane's axes, we can get a local X and Y value.
                Fixed64 y, x;
                FixedV3.Dot(ref twistMeasureAxis, ref basisA.yAxis, out y);
                FixedV3.Dot(ref twistMeasureAxis, ref basisA.xAxis, out x);
                var angle = Fixed64.FastAtan2(y, x);

                //Compute goal velocity.
                error = GetDistanceFromGoal(angle);
                Fixed64 absErrorOverDt = Fixed64.Abs(error / dt);
                Fixed64 errorReduction;
                settings.servo.springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReduction, out usedSoftness);
                biasVelocity = Fixed64.Sign(error) * MathHelper.Min(settings.servo.baseCorrectiveSpeed, absErrorOverDt) + error * errorReduction;

                biasVelocity = MathHelper.Clamp(biasVelocity, -settings.servo.maxCorrectiveVelocity, settings.servo.maxCorrectiveVelocity);
            }
            else
            {
                biasVelocity = settings.velocityMotor.goalVelocity;
                usedSoftness = settings.velocityMotor.softness / dt;
                error = F64.C0;
            }


            //The nice thing about this approach is that the jacobian entry doesn't flip.
            //Instead, the error can be negative due to the use of Atan2.
            //This is important for limits which have a unique high and low value.

            //Compute the jacobian.
            FixedV3.Add(ref basisA.primaryAxis, ref basisB.primaryAxis, out jacobianB);
            if (jacobianB.LengthSquared() < Toolbox.Epsilon)
            {
                //A nasty singularity can show up if the axes are aligned perfectly.
                //In a 'real' situation, this is impossible, so just ignore it.
                isActiveInSolver = false;
                return;
            }

            jacobianB.Normalize();
            jacobianA.X = -jacobianB.X;
            jacobianA.Y = -jacobianB.Y;
            jacobianA.Z = -jacobianB.Z;

            //Update the maximum force
            ComputeMaxForces(settings.maximumForce, dt);


            //****** EFFECTIVE MASS MATRIX ******//
            //Connection A's contribution to the mass matrix
            Fixed64 entryA;
            FixedV3 transformedAxis;
            if (connectionA.isDynamic)
            {
                BEPUMatrix3x3.Transform(ref jacobianA, ref connectionA.inertiaTensorInverse, out transformedAxis);
                FixedV3.Dot(ref transformedAxis, ref jacobianA, out entryA);
            }
            else
                entryA = F64.C0;

            //Connection B's contribution to the mass matrix
            Fixed64 entryB;
            if (connectionB.isDynamic)
            {
                BEPUMatrix3x3.Transform(ref jacobianB, ref connectionB.inertiaTensorInverse, out transformedAxis);
                FixedV3.Dot(ref transformedAxis, ref jacobianB, out entryB);
            }
            else
                entryB = F64.C0;

            //Compute the inverse mass matrix
            velocityToImpulse = F64.C1 / (usedSoftness + entryA + entryB);

            
        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //****** WARM STARTING ******//
            //Apply accumulated impulse
            FixedV3 impulse;
            if (connectionA.isDynamic)
            {
                FixedV3.Multiply(ref jacobianA, accumulatedImpulse, out impulse);
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                FixedV3.Multiply(ref jacobianB, accumulatedImpulse, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }
        }

        private Fixed64 GetDistanceFromGoal(Fixed64 angle)
        {
            Fixed64 forwardDistance;
            Fixed64 goalAngle = MathHelper.WrapAngle(settings.servo.goal);
            if (goalAngle > F64.C0)
            {
                if (angle > goalAngle)
                    forwardDistance = angle - goalAngle;
                else if (angle > F64.C0)
                    forwardDistance = MathHelper.TwoPi - goalAngle + angle;
                else //if (angle <= 0)
                    forwardDistance = MathHelper.TwoPi - goalAngle + angle;
            }
            else
            {
                if (angle < goalAngle)
                    forwardDistance = MathHelper.TwoPi - goalAngle + angle;
                else //if (angle < 0)
                    forwardDistance = angle - goalAngle;
                //else //if (currentAngle >= 0)
                //    return angle - myMinimumAngle;
            }
            return forwardDistance > MathHelper.Pi ? MathHelper.TwoPi - forwardDistance : -forwardDistance;
        }
    }
}
﻿using FixMath.NET;

namespace BEPUPhysics1int.Constraints.TwoEntity.Joints
{
    /// <summary>
    /// Prevents the connected entities from twisting relative to each other.
    /// Acts like the angular part of a universal joint.
    /// </summary>
    public class TwistJoint : BEPUJoint, I1DImpulseConstraintWithError, I1DJacobianConstraint
    {
        private FixedV3 aLocalAxisY, aLocalAxisZ;
        private Fixed64 accumulatedImpulse;
        private FixedV3 bLocalAxisY;
        private Fixed64 biasVelocity;
        private FixedV3 jacobianA, jacobianB;
        private Fixed64 error;
        private FixedV3 localAxisA;
        private FixedV3 localAxisB;
        private FixedV3 worldAxisA;
        private FixedV3 worldAxisB;
        private Fixed64 velocityToImpulse;

        /// <summary>
        /// Constructs a new constraint which prevents the connected entities from twisting relative to each other.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the WorldAxisA and WorldAxisB (or their entity-local versions).
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public TwistJoint()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a new constraint which prevents the connected entities from twisting relative to each other.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        /// <param name="axisA">Twist axis attached to the first connected entity.</param>
        /// <param name="axisB">Twist axis attached to the second connected entity.</param>
        public TwistJoint(Entity connectionA, Entity connectionB, FixedV3 axisA, FixedV3 axisB)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;
            WorldAxisA = axisA;
            WorldAxisB = axisB;
        }

        /// <summary>
        /// Gets or sets the axis attached to the first connected entity in its local space.
        /// </summary>
        public FixedV3 LocalAxisA
        {
            get { return localAxisA; }
            set
            {
                localAxisA = FixedV3.Normalize(value);
                BEPUMatrix3x3.Transform(ref localAxisA, ref connectionA.orientationMatrix, out worldAxisA);
                Initialize();
            }
        }

        /// <summary>
        /// Gets or sets the axis attached to the first connected entity in its local space.
        /// </summary>
        public FixedV3 LocalAxisB
        {
            get { return localAxisB; }
            set
            {
                localAxisB = FixedV3.Normalize(value);
                BEPUMatrix3x3.Transform(ref localAxisB, ref connectionA.orientationMatrix, out worldAxisB);
                Initialize();
            }
        }

        /// <summary>
        /// Gets or sets the axis attached to the first connected entity in world space.
        /// </summary>
        public FixedV3 WorldAxisA
        {
            get { return worldAxisA; }
            set
            {
                worldAxisA = FixedV3.Normalize(value);
                FixedQuaternion conjugate;
                FixedQuaternion.Conjugate(ref connectionA.orientation, out conjugate);
                FixedQuaternion.Transform(ref worldAxisA, ref conjugate, out localAxisA);
                Initialize();
            }
        }

        /// <summary>
        /// Gets or sets the axis attached to the first connected entity in world space.
        /// </summary>
        public FixedV3 WorldAxisB
        {
            get { return worldAxisB; }
            set
            {
                worldAxisB = FixedV3.Normalize(value);
                FixedQuaternion conjugate;
                FixedQuaternion.Conjugate(ref connectionB.orientation, out conjugate);
                FixedQuaternion.Transform(ref worldAxisB, ref conjugate, out localAxisB);
                Initialize();
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
        /// Solves for velocity.
        /// </summary>
        public override Fixed64 SolveIteration()
        {
            Fixed64 velocityA, velocityB;
            //Find the velocity contribution from each connection
            FixedV3.Dot(ref connectionA.angularVelocity, ref jacobianA, out velocityA);
            FixedV3.Dot(ref connectionB.angularVelocity, ref jacobianB, out velocityB);
            //Add in the constraint space bias velocity
            Fixed64 lambda = -(velocityA + velocityB) + biasVelocity - softness * accumulatedImpulse;

            //Transform to an impulse
            lambda *= velocityToImpulse;

            //Accumulate the impulse
            accumulatedImpulse += lambda;

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

            return (Fixed64.Abs(lambda));
        }

        /// <summary>
        /// Do any necessary computations to prepare the constraint for this frame.
        /// </summary>
        /// <param name="dt">Simulation step length.</param>
        public override void Update(Fixed64 dt)
        {
            FixedV3 aAxisY, aAxisZ;
            FixedV3 bAxisY;
            BEPUMatrix3x3.Transform(ref localAxisA, ref connectionA.orientationMatrix, out worldAxisA);
            BEPUMatrix3x3.Transform(ref aLocalAxisY, ref connectionA.orientationMatrix, out aAxisY);
            BEPUMatrix3x3.Transform(ref aLocalAxisZ, ref connectionA.orientationMatrix, out aAxisZ);
            BEPUMatrix3x3.Transform(ref localAxisB, ref connectionB.orientationMatrix, out worldAxisB);
            BEPUMatrix3x3.Transform(ref bLocalAxisY, ref connectionB.orientationMatrix, out bAxisY);

            FixedQuaternion rotation;
            FixedQuaternion.GetQuaternionBetweenNormalizedVectors(ref worldAxisB, ref worldAxisA, out rotation);

            //Transform b's 'Y' axis so that it is perpendicular with a's 'X' axis for measurement.
            FixedV3 twistMeasureAxis;
            FixedQuaternion.Transform(ref bAxisY, ref rotation, out twistMeasureAxis);

            //By dotting the measurement vector with a 2d plane's axes, we can get a local X and Y value.
            Fixed64 y, x;
            FixedV3.Dot(ref twistMeasureAxis, ref aAxisZ, out y);
            FixedV3.Dot(ref twistMeasureAxis, ref aAxisY, out x);
            error = Fixed64.FastAtan2(y, x);

            //Debug.WriteLine("Angle: " + angle);

            //The nice thing about this approach is that the jacobian entry doesn't flip.
            //Instead, the error can be negative due to the use of Atan2.
            //This is important for limits which have a unique high and low value.

            //Compute the jacobian.
            FixedV3.Add(ref worldAxisA, ref worldAxisB, out jacobianB);
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

            //****** VELOCITY BIAS ******//
            //Compute the correction velocity.
            Fixed64 errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReduction, out softness);
            biasVelocity = MathHelper.Clamp(-error * errorReduction, -maxCorrectiveVelocity, maxCorrectiveVelocity);

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
            velocityToImpulse = F64.C1 / (softness + entryA + entryB);

            
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

        /// <summary>
        /// Computes the internal bases and target relative state based on the current axes and entity states.
        /// Called automatically when setting any of the axis properties.
        /// </summary>
        public void Initialize()
        {
            //Compute a vector which is perpendicular to the axis.  It'll be added in local space to both connections.
            FixedV3 yAxis;
            FixedV3.Cross(ref worldAxisA, ref Toolbox.UpVector, out yAxis);
            Fixed64 length = yAxis.LengthSquared();
            if (length < Toolbox.Epsilon)
            {
                FixedV3.Cross(ref worldAxisA, ref Toolbox.RightVector, out yAxis);
            }
            yAxis.Normalize();

            //Put the axis into the local space of A.
            FixedQuaternion conjugate;
            FixedQuaternion.Conjugate(ref connectionA.orientation, out conjugate);
            FixedQuaternion.Transform(ref yAxis, ref conjugate, out aLocalAxisY);

            //Complete A's basis.
            FixedV3.Cross(ref localAxisA, ref aLocalAxisY, out aLocalAxisZ);

            //Rotate the axis to B since it could be arbitrarily rotated.
            FixedQuaternion rotation;
            FixedQuaternion.GetQuaternionBetweenNormalizedVectors(ref worldAxisA, ref worldAxisB, out rotation);
            FixedQuaternion.Transform(ref yAxis, ref rotation, out yAxis);

            //Put it into local space.
            FixedQuaternion.Conjugate(ref connectionB.orientation, out conjugate);
            FixedQuaternion.Transform(ref yAxis, ref conjugate, out bLocalAxisY);
        }
    }
}
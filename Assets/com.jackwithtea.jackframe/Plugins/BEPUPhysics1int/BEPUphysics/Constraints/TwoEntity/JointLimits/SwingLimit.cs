﻿using FixMath.NET;

namespace BEPUPhysics1int.Constraints.TwoEntity.JointLimits
{
    /// <summary>
    /// Keeps the angle between the axes attached to two entities below some maximum value.
    /// </summary>
    public class SwingLimit : JointLimit, I1DImpulseConstraintWithError, I1DJacobianConstraint
    {
        private Fixed64 accumulatedImpulse;
        private Fixed64 biasVelocity;
        private FixedV3 hingeAxis;
        private Fixed64 minimumCosine = F64.C1;
        private Fixed64 error;

        private FixedV3 localAxisA;

        private FixedV3 localAxisB;
        private FixedV3 worldAxisA;

        private FixedV3 worldAxisB;
        private Fixed64 velocityToImpulse;

        /// <summary>
        /// Constructs a new constraint which attempts to restrict the maximum relative angle of two entities to some value.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the WorldAxisA, WorldAxisB (or their entity-local versions) and the MaximumAngle.
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public SwingLimit()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a new constraint which attempts to restrict the maximum relative angle of two entities to some value.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        /// <param name="axisA">Axis attached to the first connected entity.</param>
        /// <param name="axisB">Axis attached to the second connected entity.</param>
        /// <param name="maximumAngle">Maximum angle between the axes allowed.</param>
        public SwingLimit(Entity connectionA, Entity connectionB, FixedV3 axisA, FixedV3 axisB, Fixed64 maximumAngle)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;
            WorldAxisA = axisA;
            WorldAxisB = axisB;
            MaximumAngle = maximumAngle;
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
            }
        }

        /// <summary>
        /// Maximum angle allowed between the two axes, from 0 to pi.
        /// </summary>
        public Fixed64 MaximumAngle
        {
            get { return Fixed64.Acos(minimumCosine); }
            set { minimumCosine = Fixed64.Cos(MathHelper.Clamp(value, F64.C0, MathHelper.Pi)); }
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
                    FixedV3 relativeVelocity;
                    FixedV3.Subtract(ref connectionA.angularVelocity, ref connectionB.angularVelocity, out relativeVelocity);
                    Fixed64 lambda;
                    FixedV3.Dot(ref relativeVelocity, ref hingeAxis, out lambda);
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
            jacobian = hingeAxis;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out FixedV3 jacobian)
        {
            jacobian = -hingeAxis;
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
        /// Applies the sequential impulse.
        /// </summary>
        public override Fixed64 SolveIteration()
        {
            Fixed64 lambda;
            FixedV3 relativeVelocity;
            FixedV3.Subtract(ref connectionA.angularVelocity, ref connectionB.angularVelocity, out relativeVelocity);
            //Transform the velocity to with the jacobian
            FixedV3.Dot(ref relativeVelocity, ref hingeAxis, out lambda);
            //Add in the constraint space bias velocity
            lambda = -lambda + biasVelocity - softness * accumulatedImpulse;

            //Transform to an impulse
            lambda *= velocityToImpulse;

            //Clamp accumulated impulse (can't go negative)
            Fixed64 previousAccumulatedImpulse = accumulatedImpulse;
            accumulatedImpulse = MathHelper.Max(accumulatedImpulse + lambda, F64.C0);
            lambda = accumulatedImpulse - previousAccumulatedImpulse;

            //Apply the impulse
            FixedV3 impulse;
            FixedV3.Multiply(ref hingeAxis, lambda, out impulse);
            if (connectionA.isDynamic)
            {
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                FixedV3.Negate(ref impulse, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }

            return (Fixed64.Abs(lambda));
        }

        /// <summary>
        /// Initializes the constraint for this frame.
        /// </summary>
        /// <param name="dt">Time since the last frame.</param>
        public override void Update(Fixed64 dt)
        {
            BEPUMatrix3x3.Transform(ref localAxisA, ref connectionA.orientationMatrix, out worldAxisA);
            BEPUMatrix3x3.Transform(ref localAxisB, ref connectionB.orientationMatrix, out worldAxisB);

            Fixed64 dot;
            FixedV3.Dot(ref worldAxisA, ref worldAxisB, out dot);

            //Keep in mind, the dot is the cosine of the angle.
            //1: 0 radians
            //0: pi/2 radians
            //-1: pi radians
            if (dot > minimumCosine)
            {
                isActiveInSolver = false;
                error = F64.C0;
                accumulatedImpulse = F64.C0;
                isLimitActive = false;
                return;
            }
            isLimitActive = true;

            //Hinge axis is actually the jacobian entry for angular A (negative angular B).
            FixedV3.Cross(ref worldAxisA, ref worldAxisB, out hingeAxis);
            Fixed64 lengthSquared = hingeAxis.LengthSquared();
            if (lengthSquared < Toolbox.Epsilon)
            {
                //They're parallel; for the sake of continuity, pick some axis which is perpendicular to both that ISN'T the zero vector.
                FixedV3.Cross(ref worldAxisA, ref Toolbox.UpVector, out hingeAxis);
                lengthSquared = hingeAxis.LengthSquared();
                if (lengthSquared < Toolbox.Epsilon)
                {
                    //That's improbable; b's world axis was apparently parallel with the up vector!
                    //So just use the right vector (it can't be parallel with both the up and right vectors).
                    FixedV3.Cross(ref worldAxisA, ref Toolbox.RightVector, out hingeAxis);
                }
            }


            Fixed64 errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReduction, out softness);

            //Further away from 0 degrees is further negative; if the dot is below the minimum cosine, it means the angle is above the maximum angle.
            error = MathHelper.Max(F64.C0, minimumCosine - dot - margin);
            biasVelocity = MathHelper.Clamp(errorReduction * error, -maxCorrectiveVelocity, maxCorrectiveVelocity);

            if (bounciness > F64.C0)
            {
                //Compute the speed around the axis.
                Fixed64 relativeSpeed;
                FixedV3 relativeVelocity;
                FixedV3.Subtract(ref connectionA.angularVelocity, ref connectionB.angularVelocity, out relativeVelocity);
                FixedV3.Dot(ref relativeVelocity, ref hingeAxis, out relativeSpeed);

                biasVelocity = MathHelper.Max(biasVelocity, ComputeBounceVelocity(-relativeSpeed));
            }

            //Connection A's contribution to the mass matrix
            Fixed64 entryA;
            FixedV3 transformedAxis;
            if (connectionA.isDynamic)
            {
                BEPUMatrix3x3.Transform(ref hingeAxis, ref connectionA.inertiaTensorInverse, out transformedAxis);
                FixedV3.Dot(ref transformedAxis, ref hingeAxis, out entryA);
            }
            else
                entryA = F64.C0;

            //Connection B's contribution to the mass matrix
            Fixed64 entryB;
            if (connectionB.isDynamic)
            {
                BEPUMatrix3x3.Transform(ref hingeAxis, ref connectionB.inertiaTensorInverse, out transformedAxis);
                FixedV3.Dot(ref transformedAxis, ref hingeAxis, out entryB);
            }
            else
                entryB = F64.C0;

            //Compute the inverse mass matrix
            velocityToImpulse = F64.C1 / (softness + entryA + entryB);


        }

        public override void ExclusiveUpdate()
        {
            //Apply accumulated impulse
            FixedV3 impulse;
            FixedV3.Multiply(ref hingeAxis, accumulatedImpulse, out impulse);
            if (connectionA.isDynamic)
            {
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                FixedV3.Negate(ref impulse, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }
        }
    }
}
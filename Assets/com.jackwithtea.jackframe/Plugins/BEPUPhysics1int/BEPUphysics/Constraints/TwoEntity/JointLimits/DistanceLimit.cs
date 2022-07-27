using System;
using BEPUphysics.Entities;

using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Constraints.TwoEntity.JointLimits
{
    /// <summary>
    /// A modified distance constraint allowing a range of lengths between two anchor points.
    /// </summary>
    public class DistanceLimit : JointLimit, I1DImpulseConstraintWithError, I1DJacobianConstraint
    {
        private Fixed64 accumulatedImpulse;
        private FixedV3 anchorA;

        private FixedV3 anchorB;
        private Fixed64 biasVelocity;
        private FixedV3 jAngularA, jAngularB;
        private FixedV3 jLinearA, jLinearB;
        private Fixed64 error;

        private FixedV3 localAnchorA;

        private FixedV3 localAnchorB;

        /// <summary>
        /// Maximum distance allowed between the anchors.
        /// </summary>
        protected Fixed64 maximumLength;

        /// <summary>
        /// Minimum distance maintained between the anchors.
        /// </summary>
        protected Fixed64 minimumLength;


        private FixedV3 offsetA, offsetB;
        private Fixed64 velocityToImpulse;

        /// <summary>
        /// Constructs a distance limit joint.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the WorldAnchorA and WorldAnchorB (or their entity-local versions)
        /// and the MinimumLength and MaximumLength.
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public DistanceLimit()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a distance limit joint.
        /// </summary>
        /// <param name="connectionA">First body connected to the distance limit.</param>
        /// <param name="connectionB">Second body connected to the distance limit.</param>
        /// <param name="anchorA">Connection to the spring from the first connected body in world space.</param>
        /// <param name="anchorB"> Connection to the spring from the second connected body in world space.</param>
        /// <param name="minimumLength">Minimum distance maintained between the anchors.</param>
        /// <param name="maximumLength">Maximum distance allowed between the anchors.</param>
        public DistanceLimit(Entity connectionA, Entity connectionB, FixedV3 anchorA, FixedV3 anchorB, Fixed64 minimumLength, Fixed64 maximumLength)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;
            MinimumLength = minimumLength;
            MaximumLength = maximumLength;

            WorldAnchorA = anchorA;
            WorldAnchorB = anchorB;
        }

        /// <summary>
        /// Gets or sets the first entity's connection point in local space.
        /// </summary>
        public FixedV3 LocalAnchorA
        {
            get { return localAnchorA; }
            set
            {
                localAnchorA = value;
                BEPUMatrix3x3.Transform(ref localAnchorA, ref connectionA.orientationMatrix, out anchorA);
                anchorA += connectionA.position;
            }
        }

        /// <summary>
        /// Gets or sets the first entity's connection point in local space.
        /// </summary>
        public FixedV3 LocalAnchorB
        {
            get { return localAnchorB; }
            set
            {
                localAnchorB = value;
                BEPUMatrix3x3.Transform(ref localAnchorB, ref connectionB.orientationMatrix, out anchorB);
                anchorB += connectionB.position;
            }
        }

        /// <summary>
        /// Gets or sets the maximum distance allowed between the anchors.
        /// </summary>
        public Fixed64 MaximumLength
        {
            get { return maximumLength; }
            set
            {
                maximumLength = MathHelper.Max(F64.C0, value);
                minimumLength = MathHelper.Min(minimumLength, maximumLength);
            }
        }

        /// <summary>
        /// Gets or sets the minimum distance maintained between the anchors.
        /// </summary>
        public Fixed64 MinimumLength
        {
            get { return minimumLength; }
            set
            {
                minimumLength = MathHelper.Max(F64.C0, value);
                maximumLength = MathHelper.Max(minimumLength, maximumLength);
            }
        }

        /// <summary>
        /// Gets or sets the connection to the distance constraint from the first connected body in world space.
        /// </summary>
        public FixedV3 WorldAnchorA
        {
            get { return anchorA; }
            set
            {
                anchorA = value;
                localAnchorA = FixedQuaternion.Transform(anchorA - connectionA.position, FixedQuaternion.Conjugate(connectionA.orientation));
            }
        }

        /// <summary>
        /// Gets or sets the connection to the distance constraint from the second connected body in world space.
        /// </summary>
        public FixedV3 WorldAnchorB
        {
            get { return anchorB; }
            set
            {
                anchorB = value;
                localAnchorB = FixedQuaternion.Transform(anchorB - connectionB.position, FixedQuaternion.Conjugate(connectionB.orientation));
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
            outputMassMatrix = velocityToImpulse;
        }

        #endregion

        /// <summary>
        /// Calculates and applies corrective impulses.
        /// Called automatically by space.
        /// </summary>
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
            lambda *= velocityToImpulse;

            //Clamp accumulated impulse (can't go negative)
            Fixed64 previousAccumulatedImpulse = accumulatedImpulse;
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

        /// <summary>
        /// Calculates necessary information for velocity solving.
        /// </summary>
        /// <param name="dt">Time in seconds since the last update.</param>
        public override void Update(Fixed64 dt)
        {
            //Transform the anchors and offsets into world space.
            BEPUMatrix3x3.Transform(ref localAnchorA, ref connectionA.orientationMatrix, out offsetA);
            BEPUMatrix3x3.Transform(ref localAnchorB, ref connectionB.orientationMatrix, out offsetB);
            FixedV3.Add(ref connectionA.position, ref offsetA, out anchorA);
            FixedV3.Add(ref connectionB.position, ref offsetB, out anchorB);

            //Compute the distance.
            FixedV3 separation;
            FixedV3.Subtract(ref anchorB, ref anchorA, out separation);
            Fixed64 distance = separation.Length();
            if (distance <= maximumLength && distance >= minimumLength)
            {
                isActiveInSolver = false;
                accumulatedImpulse = F64.C0;
                error = F64.C0;
                isLimitActive = false;
                return;
            }
            isLimitActive = true;


            //Compute jacobians
            if (distance > maximumLength)
            {
                //If it's beyond the max, all of the jacobians are reversed compared to what they are when it's below the min.
                if (distance > Toolbox.Epsilon)
                {
                    jLinearA.X = separation.X / distance;
                    jLinearA.Y = separation.Y / distance;
                    jLinearA.Z = separation.Z / distance;
                }
                else
                    jLinearB = Toolbox.ZeroVector;

                jLinearB.X = -jLinearA.X;
                jLinearB.Y = -jLinearA.Y;
                jLinearB.Z = -jLinearA.Z;

                FixedV3.Cross(ref jLinearA, ref offsetA, out jAngularA);
                //Still need to negate angular A.  It's done after the effective mass matrix.
                FixedV3.Cross(ref jLinearA, ref offsetB, out jAngularB);
            }
            else
            {
                if (distance > Toolbox.Epsilon)
                {
                    jLinearB.X = separation.X / distance;
                    jLinearB.Y = separation.Y / distance;
                    jLinearB.Z = separation.Z / distance;
                }
                else
                    jLinearB = Toolbox.ZeroVector;

                jLinearA.X = -jLinearB.X;
                jLinearA.Y = -jLinearB.Y;
                jLinearA.Z = -jLinearB.Z;

                FixedV3.Cross(ref offsetA, ref jLinearB, out jAngularA);
                //Still need to negate angular A.  It's done after the effective mass matrix.
                FixedV3.Cross(ref offsetB, ref jLinearB, out jAngularB);
            }


            //Debug.WriteLine("BiasVelocity: " + biasVelocity);


            //Compute effective mass matrix
            if (connectionA.isDynamic && connectionB.isDynamic)
            {
                FixedV3 aAngular;
                BEPUMatrix3x3.Transform(ref jAngularA, ref connectionA.localInertiaTensorInverse, out aAngular);
                FixedV3.Cross(ref aAngular, ref offsetA, out aAngular);
                FixedV3 bAngular;
                BEPUMatrix3x3.Transform(ref jAngularB, ref connectionB.localInertiaTensorInverse, out bAngular);
                FixedV3.Cross(ref bAngular, ref offsetB, out bAngular);
                FixedV3.Add(ref aAngular, ref bAngular, out aAngular);
                FixedV3.Dot(ref aAngular, ref jLinearB, out velocityToImpulse);
                velocityToImpulse += connectionA.inverseMass + connectionB.inverseMass;
            }
            else if (connectionA.isDynamic)
            {
                FixedV3 aAngular;
                BEPUMatrix3x3.Transform(ref jAngularA, ref connectionA.localInertiaTensorInverse, out aAngular);
                FixedV3.Cross(ref aAngular, ref offsetA, out aAngular);
                FixedV3.Dot(ref aAngular, ref jLinearB, out velocityToImpulse);
                velocityToImpulse += connectionA.inverseMass;
            }
            else if (connectionB.isDynamic)
            {
                FixedV3 bAngular;
                BEPUMatrix3x3.Transform(ref jAngularB, ref connectionB.localInertiaTensorInverse, out bAngular);
                FixedV3.Cross(ref bAngular, ref offsetB, out bAngular);
                FixedV3.Dot(ref bAngular, ref jLinearB, out velocityToImpulse);
                velocityToImpulse += connectionB.inverseMass;
            }
            else
            {
                //No point in trying to solve with two kinematics.
                isActiveInSolver = false;
                accumulatedImpulse = F64.C0;
                return;
            }

            Fixed64 errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReduction, out softness);

            velocityToImpulse = F64.C1 / (softness + velocityToImpulse);
            //Finish computing jacobian; it's down here as an optimization (since it didn't need to be negated in mass matrix)
            jAngularA.X = -jAngularA.X;
            jAngularA.Y = -jAngularA.Y;
            jAngularA.Z = -jAngularA.Z;

            //Compute bias velocity
            if (distance > maximumLength)
                error = MathHelper.Max(F64.C0, distance - maximumLength - Margin);
            else
                error = MathHelper.Max(F64.C0, minimumLength - Margin - distance);
            biasVelocity = MathHelper.Min(errorReduction * error, maxCorrectiveVelocity);
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
                biasVelocity = MathHelper.Max(biasVelocity, ComputeBounceVelocity(-relativeVelocity));
            }


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
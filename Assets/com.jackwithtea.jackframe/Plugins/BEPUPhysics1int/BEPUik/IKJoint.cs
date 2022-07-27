﻿using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Connects two bones together.
    /// </summary>
    public abstract class IKJoint : IKConstraint
    {
        /// <summary>
        /// Gets the first bone connected by this joint.
        /// </summary>
        public Bone ConnectionA { get; private set; }
        /// <summary>
        /// Gets the second bone connected by this joint.
        /// </summary>
        public Bone ConnectionB { get; private set; }

        /// <summary>
        /// Gets whether or not the joint is a member of the active set as determined by the last IK solver execution.
        /// </summary>
        public bool IsActive { get; internal set; }

        bool enabled;
        /// <summary>
        /// Gets or sets whether or not this joint is enabled. If set to true, this joint will be a part of
        /// the joint graph and will undergo solving. If set to false, this joint will be removed from the connected bones and will no longer be traversable.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                //The bones must know which joints they are associated with so that the bone-joint graph can be traversed.
                if (enabled && !value)
                {
                    ConnectionA.joints.Remove(this);
                    ConnectionB.joints.Remove(this);
                }
                else if (!enabled && value)
                {
                    ConnectionA.joints.Add(this);
                    ConnectionB.joints.Add(this);
                }
                enabled = value;
            }
        }

        protected IKJoint(Bone connectionA, Bone connectionB)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;
            Enabled = true;
        }



        internal FixedV3 velocityBias;
        internal BEPUMatrix3x3 linearJacobianA;
        internal BEPUMatrix3x3 angularJacobianA;
        internal BEPUMatrix3x3 linearJacobianB;
        internal BEPUMatrix3x3 angularJacobianB;
        internal BEPUMatrix3x3 effectiveMass;

        internal FixedV3 accumulatedImpulse;





        protected internal override void ComputeEffectiveMass()
        {
            //For all constraints, the effective mass matrix is 1 / (J * M^-1 * JT).
            //For two bone constraints, J has 4 3x3 matrices. M^-1 (W below) is a 12x12 matrix with 4 3x3 block diagonal matrices.
            //To compute the whole denominator,
            BEPUMatrix3x3 linearW;
            BEPUMatrix3x3 linearA, angularA, linearB, angularB;

            if (!ConnectionA.Pinned)
            {
                BEPUMatrix3x3.CreateScale(ConnectionA.inverseMass, out linearW);
                BEPUMatrix3x3.Multiply(ref linearJacobianA, ref linearW, out linearA); //Compute J * M^-1 for linear component
                BEPUMatrix3x3.MultiplyByTransposed(ref linearA, ref linearJacobianA, out linearA); //Compute (J * M^-1) * JT for linear component

                BEPUMatrix3x3.Multiply(ref angularJacobianA, ref ConnectionA.inertiaTensorInverse, out angularA); //Compute J * M^-1 for angular component
                BEPUMatrix3x3.MultiplyByTransposed(ref angularA, ref angularJacobianA, out angularA); //Compute (J * M^-1) * JT for angular component
            }
            else
            {
                //Treat pinned bones as if they have infinite inertia.
                linearA = new BEPUMatrix3x3();
                angularA = new BEPUMatrix3x3();
            }

            if (!ConnectionB.Pinned)
            {
                BEPUMatrix3x3.CreateScale(ConnectionB.inverseMass, out linearW);
                BEPUMatrix3x3.Multiply(ref linearJacobianB, ref linearW, out linearB); //Compute J * M^-1 for linear component
                BEPUMatrix3x3.MultiplyByTransposed(ref linearB, ref linearJacobianB, out linearB); //Compute (J * M^-1) * JT for linear component

                BEPUMatrix3x3.Multiply(ref angularJacobianB, ref ConnectionB.inertiaTensorInverse, out angularB); //Compute J * M^-1 for angular component
                BEPUMatrix3x3.MultiplyByTransposed(ref angularB, ref angularJacobianB, out angularB); //Compute (J * M^-1) * JT for angular component
            }
            else
            {
                //Treat pinned bones as if they have infinite inertia.
                linearB = new BEPUMatrix3x3();
                angularB = new BEPUMatrix3x3();
            }

            //A nice side effect of the block diagonal nature of M^-1 is that the above separated components are now combined into the complete denominator matrix by addition!
            BEPUMatrix3x3.Add(ref linearA, ref angularA, out effectiveMass);
            BEPUMatrix3x3.Add(ref effectiveMass, ref linearB, out effectiveMass);
            BEPUMatrix3x3.Add(ref effectiveMass, ref angularB, out effectiveMass);

            //Incorporate the constraint softness into the effective mass denominator. This pushes the matrix away from singularity.
            //Softness will also be incorporated into the velocity solve iterations to complete the implementation.
            if (effectiveMass.M11 != F64.C0)
                effectiveMass.M11 += softness;
            if (effectiveMass.M22 != F64.C0)
                effectiveMass.M22 += softness;
            if (effectiveMass.M33 != F64.C0)
                effectiveMass.M33 += softness;

            //Invert! Takes us from J * M^-1 * JT to 1 / (J * M^-1 * JT).
            BEPUMatrix3x3.AdaptiveInvert(ref effectiveMass, out effectiveMass);

        }

        protected internal override void WarmStart()
        {
            //Take the accumulated impulse and transform it into world space impulses using the jacobians by P = JT * lambda
            //(where P is the impulse, JT is the transposed jacobian matrix, and lambda is the accumulated impulse).
            //Recall the jacobian takes impulses from world space into constraint space, and transpose takes them from constraint space into world space.

            FixedV3 impulse;
            if (!ConnectionA.Pinned) //Treat pinned elements as if they have infinite inertia.
            {
                //Compute and apply linear impulse for A.
                BEPUMatrix3x3.Transform(ref accumulatedImpulse, ref linearJacobianA, out impulse);
                ConnectionA.ApplyLinearImpulse(ref impulse);

                //Compute and apply angular impulse for A.
                BEPUMatrix3x3.Transform(ref accumulatedImpulse, ref angularJacobianA, out impulse);
                ConnectionA.ApplyAngularImpulse(ref impulse);
            }

            if (!ConnectionB.Pinned) //Treat pinned elements as if they have infinite inertia.
            {
                //Compute and apply linear impulse for B.
                BEPUMatrix3x3.Transform(ref accumulatedImpulse, ref linearJacobianB, out impulse);
                ConnectionB.ApplyLinearImpulse(ref impulse);

                //Compute and apply angular impulse for B.
                BEPUMatrix3x3.Transform(ref accumulatedImpulse, ref angularJacobianB, out impulse);
                ConnectionB.ApplyAngularImpulse(ref impulse);
            }
        }

        protected internal override void SolveVelocityIteration()
        {
            //Compute the 'relative' linear and angular velocities. For single bone constraints, it's based entirely on the one bone's velocities!
            //They have to be pulled into constraint space first to compute the necessary impulse, though.
            FixedV3 linearContributionA;
            BEPUMatrix3x3.TransformTranspose(ref ConnectionA.linearVelocity, ref linearJacobianA, out linearContributionA);
            FixedV3 angularContributionA;
            BEPUMatrix3x3.TransformTranspose(ref ConnectionA.angularVelocity, ref angularJacobianA, out angularContributionA);
            FixedV3 linearContributionB;
            BEPUMatrix3x3.TransformTranspose(ref ConnectionB.linearVelocity, ref linearJacobianB, out linearContributionB);
            FixedV3 angularContributionB;
            BEPUMatrix3x3.TransformTranspose(ref ConnectionB.angularVelocity, ref angularJacobianB, out angularContributionB);

            //The constraint velocity error will be the velocity we try to remove.
            FixedV3 constraintVelocityError;
            FixedV3.Add(ref linearContributionA, ref angularContributionA, out constraintVelocityError);
            FixedV3.Add(ref constraintVelocityError, ref linearContributionB, out constraintVelocityError);
            FixedV3.Add(ref constraintVelocityError, ref angularContributionB, out constraintVelocityError);
            //However, we need to take into account two extra sources of velocities which modify our target velocity away from zero.
            //First, the velocity bias from position correction:
            FixedV3.Subtract(ref constraintVelocityError, ref velocityBias, out constraintVelocityError);
            //And second, the bias from softness:
            FixedV3 softnessBias;
            FixedV3.Multiply(ref accumulatedImpulse, -softness, out softnessBias);
            FixedV3.Subtract(ref constraintVelocityError, ref softnessBias, out constraintVelocityError);

            //By now, the constraint velocity error contains all the velocity we want to get rid of.
            //Convert it into an impulse using the effective mass matrix.
            FixedV3 constraintSpaceImpulse;
            BEPUMatrix3x3.Transform(ref constraintVelocityError, ref effectiveMass, out constraintSpaceImpulse);

            FixedV3.Negate(ref constraintSpaceImpulse, out constraintSpaceImpulse);

            //Add the constraint space impulse to the accumulated impulse so that warm starting and softness work properly.
            FixedV3 preadd = accumulatedImpulse;
            FixedV3.Add(ref constraintSpaceImpulse, ref accumulatedImpulse, out accumulatedImpulse);
            //But wait! The accumulated impulse may exceed this constraint's capacity! Check to make sure!
            Fixed64 impulseSquared = accumulatedImpulse.LengthSquared();
            if (impulseSquared > maximumImpulseSquared)
            {
                //Oops! Clamp that down.
                FixedV3.Multiply(ref accumulatedImpulse, maximumImpulse / Fixed64.Sqrt(impulseSquared), out accumulatedImpulse);
                //Update the impulse based upon the clamped accumulated impulse and the original, pre-add accumulated impulse.
                FixedV3.Subtract(ref accumulatedImpulse, ref preadd, out constraintSpaceImpulse);
            }

            //The constraint space impulse now represents the impulse we want to apply to the bone... but in constraint space.
            //Bring it out to world space using the transposed jacobian.
            if (!ConnectionA.Pinned)//Treat pinned elements as if they have infinite inertia.
            {
                FixedV3 linearImpulseA;
                BEPUMatrix3x3.Transform(ref constraintSpaceImpulse, ref linearJacobianA, out linearImpulseA);
                FixedV3 angularImpulseA;
                BEPUMatrix3x3.Transform(ref constraintSpaceImpulse, ref angularJacobianA, out angularImpulseA);

                //Apply them!
                ConnectionA.ApplyLinearImpulse(ref linearImpulseA);
                ConnectionA.ApplyAngularImpulse(ref angularImpulseA);
            }
            if (!ConnectionB.Pinned)//Treat pinned elements as if they have infinite inertia.
            {
                FixedV3 linearImpulseB;
                BEPUMatrix3x3.Transform(ref constraintSpaceImpulse, ref linearJacobianB, out linearImpulseB);
                FixedV3 angularImpulseB;
                BEPUMatrix3x3.Transform(ref constraintSpaceImpulse, ref angularJacobianB, out angularImpulseB);

                //Apply them!
                ConnectionB.ApplyLinearImpulse(ref linearImpulseB);
                ConnectionB.ApplyAngularImpulse(ref angularImpulseB);
            }

        }

        protected internal override void ClearAccumulatedImpulses()
        {
            accumulatedImpulse = new FixedV3();
        }
    }
}

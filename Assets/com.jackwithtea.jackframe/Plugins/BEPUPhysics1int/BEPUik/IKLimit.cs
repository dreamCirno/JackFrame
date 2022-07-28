using System;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Connects two bones together.
    /// </summary>
    public abstract class IKLimit : IKJoint
    {
        protected IKLimit(Bone connectionA, Bone connectionB)
            : base(connectionA, connectionB)
        {
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
            //Limits can only apply positive impulses.
            FixedV3.Max(ref Toolbox.ZeroVector, ref accumulatedImpulse, out accumulatedImpulse);
            //But wait! The accumulated impulse may exceed this constraint's capacity! Check to make sure!
            Fixed64 impulseSquared = accumulatedImpulse.LengthSquared();
            if (impulseSquared > maximumImpulseSquared)
            {
                //Oops! Clamp that down.
                FixedV3.Multiply(ref accumulatedImpulse, maximumImpulse / Fixed64.Sqrt(impulseSquared), out accumulatedImpulse);
            }
            //Update the impulse based upon the clamped accumulated impulse and the original, pre-add accumulated impulse.
            FixedV3.Subtract(ref accumulatedImpulse, ref preadd, out constraintSpaceImpulse);

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

    }
}

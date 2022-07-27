using FixMath.NET;

namespace BEPUPhysics1int.Constraints.TwoEntity.Motors
{
    /// <summary>
    /// Superclass of constraints which do work and change the velocity of connected entities, but have no specific position target.
    /// </summary>
    public abstract class Motor : TwoEntityConstraint
    {
        protected Fixed64 maxForceDt = Fixed64.MaxValue;
        protected Fixed64 maxForceDtSquared = Fixed64.MaxValue;

        /// <summary>
        /// Softness divided by the timestep to maintain timestep independence.
        /// </summary>
        internal Fixed64 usedSoftness;

        /// <summary>
        /// Computes the maxForceDt and maxForceDtSquared fields.
        /// </summary>
        protected void ComputeMaxForces(Fixed64 maxForce, Fixed64 dt)
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
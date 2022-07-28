using FixMath.NET;

namespace BEPUPhysics1int.Constraints.SingleEntity
{
    /// <summary>
    /// Prevents the target entity from moving faster than the specified speeds.
    /// </summary>
    public class MaximumAngularSpeedConstraint : SingleEntityConstraint, I3DImpulseConstraint
    {
        private BEPUMatrix3x3 effectiveMassMatrix;
        private Fixed64 maxForceDt = Fixed64.MaxValue;
        private Fixed64 maxForceDtSquared = Fixed64.MaxValue;
        private FixedV3 accumulatedImpulse;
        private Fixed64 maximumForce = Fixed64.MaxValue;
        private Fixed64 maximumSpeed;
        private Fixed64 maximumSpeedSquared;

        private Fixed64 softness = (Fixed64).00001m;
        private Fixed64 usedSoftness;

        /// <summary>
        /// Constructs a maximum speed constraint.
        /// Set its Entity and MaximumSpeed to complete the configuration.
        /// IsActive also starts as false with this constructor.
        /// </summary>
        public MaximumAngularSpeedConstraint()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a maximum speed constraint.
        /// </summary>
        /// <param name="e">Affected entity.</param>
        /// <param name="maxSpeed">Maximum angular speed allowed.</param>
        public MaximumAngularSpeedConstraint(Entity e, Fixed64 maxSpeed)
        {
            Entity = e;
            MaximumSpeed = maxSpeed;
        }

        /// <summary>
        /// Gets and sets the maximum impulse that the constraint will attempt to apply when satisfying its requirements.
        /// This field can be used to simulate friction in a constraint.
        /// </summary>
        public Fixed64 MaximumForce
        {
            get
            {
                if (maximumForce > F64.C0)
                {
                    return maximumForce;
                }
                return F64.C0;
            }
            set { maximumForce = value >= F64.C0 ? value : F64.C0; }
        }

        /// <summary>
        /// Gets or sets the maximum angular speed that this constraint allows.
        /// </summary>
        public Fixed64 MaximumSpeed
        {
            get { return maximumSpeed; }
            set
            {
                maximumSpeed = MathHelper.Max(F64.C0, value);
                maximumSpeedSquared = maximumSpeed * maximumSpeed;
            }
        }


        /// <summary>
        /// Gets and sets the softness of this constraint.
        /// Higher values of softness allow the constraint to be violated more.
        /// Must be greater than zero.
        /// Sometimes, if a joint system is unstable, increasing the softness of the involved constraints will make it settle down.
        /// For motors, softness can be used to implement damping.  For a damping constant k, the appropriate softness is 1/k.
        /// </summary>
        public Fixed64 Softness
        {
            get { return softness; }
            set { softness = MathHelper.Max(F64.C0, value); }
        }

        #region I3DImpulseConstraint Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        FixedV3 I3DImpulseConstraint.RelativeVelocity
        {
            get { return entity.angularVelocity; }
        }

        /// <summary>
        /// Gets the total impulse applied by the constraint.
        /// </summary>
        public FixedV3 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        #endregion

        /// <summary>
        /// Calculates and applies corrective impulses.
        /// Called automatically by space.
        /// </summary>
        public override Fixed64 SolveIteration()
        {
            Fixed64 angularSpeed = entity.angularVelocity.LengthSquared();
            if (angularSpeed > maximumSpeedSquared)
            {
                angularSpeed = Fixed64.Sqrt(angularSpeed);
                FixedV3 impulse;
                //divide by angularSpeed to normalize the velocity.
                //Multiply by angularSpeed - maximumSpeed to get the 'velocity change vector.'
                FixedV3.Multiply(ref entity.angularVelocity, -(angularSpeed - maximumSpeed) / angularSpeed, out impulse);

                //incorporate softness
                FixedV3 softnessImpulse;
                FixedV3.Multiply(ref accumulatedImpulse, usedSoftness, out softnessImpulse);
                FixedV3.Subtract(ref impulse, ref softnessImpulse, out impulse);

                //Transform into impulse
                BEPUMatrix3x3.Transform(ref impulse, ref effectiveMassMatrix, out impulse);


                //Accumulate
                FixedV3 previousAccumulatedImpulse = accumulatedImpulse;
                FixedV3.Add(ref accumulatedImpulse, ref impulse, out accumulatedImpulse);
                Fixed64 forceMagnitude = accumulatedImpulse.LengthSquared();
                if (forceMagnitude > maxForceDtSquared)
                {
                    //max / impulse gives some value 0 < x < 1.  Basically, normalize the vector (divide by the length) and scale by the maximum.
                    Fixed64 multiplier = maxForceDt / Fixed64.Sqrt(forceMagnitude);
                    accumulatedImpulse.X *= multiplier;
                    accumulatedImpulse.Y *= multiplier;
                    accumulatedImpulse.Z *= multiplier;

                    //Since the limit was exceeded by this corrective impulse, limit it so that the accumulated impulse remains constrained.
                    impulse.X = accumulatedImpulse.X - previousAccumulatedImpulse.X;
                    impulse.Y = accumulatedImpulse.Y - previousAccumulatedImpulse.Y;
                    impulse.Z = accumulatedImpulse.Z - previousAccumulatedImpulse.Z;
                }

                entity.ApplyAngularImpulse(ref impulse);


                return (Fixed64.Abs(impulse.X) + Fixed64.Abs(impulse.Y) + Fixed64.Abs(impulse.Z));
            }

            return F64.C0;
        }

        /// <summary>
        /// Calculates necessary information for velocity solving.
        /// Called automatically by space.
        /// </summary>
        /// <param name="dt">Time in seconds since the last update.</param>
        public override void Update(Fixed64 dt)
        {
            usedSoftness = softness / dt;

            effectiveMassMatrix = entity.inertiaTensorInverse;

            effectiveMassMatrix.M11 += usedSoftness;
            effectiveMassMatrix.M22 += usedSoftness;
            effectiveMassMatrix.M33 += usedSoftness;

            BEPUMatrix3x3.Invert(ref effectiveMassMatrix, out effectiveMassMatrix);

            //Determine maximum force
            if (maximumForce < Fixed64.MaxValue)
            {
                maxForceDt = maximumForce * dt;
                maxForceDtSquared = maxForceDt * maxForceDt;
            }
            else
            {
                maxForceDt = Fixed64.MaxValue;
                maxForceDtSquared = Fixed64.MaxValue;
            }

        }


        public override void ExclusiveUpdate()
        {

            //Can't do warmstarting due to the strangeness of this constraint (not based on a position error, nor is it really a motor).
            accumulatedImpulse = Toolbox.ZeroVector;
        }
    }
}
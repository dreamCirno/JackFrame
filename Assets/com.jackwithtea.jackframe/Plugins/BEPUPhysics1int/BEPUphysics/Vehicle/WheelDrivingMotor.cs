﻿using BEPUPhysics1int.Constraints;
using BEPUPhysics1int.Materials;
using FixMath.NET;

namespace BEPUPhysics1int.Vehicle
{
    /// <summary>
    /// Handles a wheel's driving force for a vehicle.
    /// </summary>
    public class WheelDrivingMotor : ISolverSettings
    {
        #region Static Stuff

        /// <summary>
        /// Default blender used by WheelSlidingFriction constraints.
        /// </summary>
        public static WheelFrictionBlender DefaultGripFrictionBlender;

        static WheelDrivingMotor()
        {
            DefaultGripFrictionBlender = BlendFriction;
        }

        /// <summary>
        /// Function which takes the friction values from a wheel and a supporting material and computes the blended friction.
        /// </summary>
        /// <param name="wheelFriction">Friction coefficient associated with the wheel.</param>
        /// <param name="materialFriction">Friction coefficient associated with the support material.</param>
        /// <param name="usingKineticFriction">True if the friction coefficients passed into the blender are kinetic coefficients, false otherwise.</param>
        /// <param name="wheel">Wheel being blended.</param>
        /// <returns>Blended friction coefficient.</returns>
        public static Fixed64 BlendFriction(Fixed64 wheelFriction, Fixed64 materialFriction, bool usingKineticFriction, Wheel wheel)
        {
            return wheelFriction * materialFriction;
        }

        #endregion

        internal Fixed64 accumulatedImpulse;

        //Fix64 linearBX, linearBY, linearBZ;
        internal Fixed64 angularAX, angularAY, angularAZ;
        internal Fixed64 angularBX, angularBY, angularBZ;
        internal bool isActive = true;
        internal Fixed64 linearAX, linearAY, linearAZ;
        private Fixed64 currentFrictionCoefficient;
        internal FixedV3 forceAxis;
        private Fixed64 gripFriction;
        private WheelFrictionBlender gripFrictionBlender = DefaultGripFrictionBlender;
        private Fixed64 maxMotorForceDt;
        private Fixed64 maximumBackwardForce = Fixed64.MaxValue;
        private Fixed64 maximumForwardForce = Fixed64.MaxValue;
        internal SolverSettings solverSettings = new SolverSettings();
        private Fixed64 targetSpeed;
        private Wheel wheel;
        internal int numIterationsAtZeroImpulse;
        private Entity vehicleEntity, supportEntity;

        //Inverse effective mass matrix
        internal Fixed64 velocityToImpulse;
        private bool supportIsDynamic;

        /// <summary>
        /// Constructs a new wheel motor.
        /// </summary>
        /// <param name="gripFriction">Friction coefficient of the wheel.  Blended with the ground's friction coefficient and normal force to determine a maximum force.</param>
        /// <param name="maximumForwardForce">Maximum force that the wheel motor can apply when driving forward (a target speed greater than zero).</param>
        /// <param name="maximumBackwardForce">Maximum force that the wheel motor can apply when driving backward (a target speed less than zero).</param>
        public WheelDrivingMotor(Fixed64 gripFriction, Fixed64 maximumForwardForce, Fixed64 maximumBackwardForce)
        {
            GripFriction = gripFriction;
            MaximumForwardForce = maximumForwardForce;
            MaximumBackwardForce = maximumBackwardForce;
        }

        internal WheelDrivingMotor(Wheel wheel)
        {
            Wheel = wheel;
        }

        /// <summary>
        /// Gets the coefficient of grip friction between the wheel and support.
        /// This coefficient is the blended result of the supporting entity's friction and the wheel's friction.
        /// </summary>
        public Fixed64 BlendedCoefficient
        {
            get { return currentFrictionCoefficient; }
        }

        /// <summary>
        /// Gets the axis along which the driving forces are applied.
        /// </summary>
        public FixedV3 ForceAxis
        {
            get { return forceAxis; }
        }

        /// <summary>
        /// Gets or sets the coefficient of forward-backward gripping friction for this wheel.
        /// This coefficient and the supporting entity's coefficient of friction will be 
        /// taken into account to determine the used coefficient at any given time.
        /// </summary>
        public Fixed64 GripFriction
        {
            get { return gripFriction; }
            set { gripFriction = MathHelper.Max(value, F64.C0); }
        }

        /// <summary>
        /// Gets or sets the function used to blend the supporting entity's friction and the wheel's friction.
        /// </summary>
        public WheelFrictionBlender GripFrictionBlender
        {
            get { return gripFrictionBlender; }
            set { gripFrictionBlender = value; }
        }

        /// <summary>
        /// Gets or sets the maximum force that the wheel motor can apply when driving backward (a target speed less than zero).
        /// </summary>
        public Fixed64 MaximumBackwardForce
        {
            get { return maximumBackwardForce; }
            set { maximumBackwardForce = value; }
        }

        /// <summary>
        /// Gets or sets the maximum force that the wheel motor can apply when driving forward (a target speed greater than zero).
        /// </summary>
        public Fixed64 MaximumForwardForce
        {
            get { return maximumForwardForce; }
            set { maximumForwardForce = value; }
        }

        /// <summary>
        /// Gets or sets the target speed of this wheel.
        /// </summary>
        public Fixed64 TargetSpeed
        {
            get { return targetSpeed; }
            set { targetSpeed = value; }
        }

        /// <summary>
        /// Gets the force this wheel's motor is applying.
        /// </summary>
        public Fixed64 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the wheel that this motor applies to.
        /// </summary>
        public Wheel Wheel
        {
            get { return wheel; }
            internal set { wheel = value; }
        }

        #region ISolverSettings Members

        /// <summary>
        /// Gets the solver settings used by this wheel constraint.
        /// </summary>
        public SolverSettings SolverSettings
        {
            get { return solverSettings; }
        }

        #endregion

        /// <summary>
        /// Gets the relative velocity between the ground and wheel.
        /// </summary>
        /// <returns>Relative velocity between the ground and wheel.</returns>
        public Fixed64 RelativeVelocity
        {
            get
            {
                Fixed64 velocity = F64.C0;
                if (vehicleEntity != null)
                    velocity += vehicleEntity.linearVelocity.X * linearAX + vehicleEntity.linearVelocity.Y * linearAY + vehicleEntity.linearVelocity.Z * linearAZ +
                                  vehicleEntity.angularVelocity.X * angularAX + vehicleEntity.angularVelocity.Y * angularAY + vehicleEntity.angularVelocity.Z * angularAZ;
                if (supportEntity != null)
                    velocity += -supportEntity.linearVelocity.X * linearAX - supportEntity.linearVelocity.Y * linearAY - supportEntity.linearVelocity.Z * linearAZ +
                                supportEntity.angularVelocity.X * angularBX + supportEntity.angularVelocity.Y * angularBY + supportEntity.angularVelocity.Z * angularBZ;
                return velocity;
            }
        }

        internal Fixed64 ApplyImpulse()
        {
            //Compute relative velocity
            Fixed64 lambda = (RelativeVelocity
                            - targetSpeed) //Add in the extra goal speed
                           * velocityToImpulse; //convert to impulse


            //Clamp accumulated impulse
            Fixed64 previousAccumulatedImpulse = accumulatedImpulse;
            accumulatedImpulse += lambda;
            //Don't brake, and take into account the motor's maximum force.
            if (targetSpeed > F64.C0)
                accumulatedImpulse = MathHelper.Clamp(accumulatedImpulse, F64.C0, maxMotorForceDt); //MathHelper.Min(MathHelper.Max(accumulatedImpulse, 0), myMaxMotorForceDt);
            else if (targetSpeed < F64.C0)
                accumulatedImpulse = MathHelper.Clamp(accumulatedImpulse, maxMotorForceDt, F64.C0); //MathHelper.Max(MathHelper.Min(accumulatedImpulse, 0), myMaxMotorForceDt);
            else
                accumulatedImpulse = F64.C0;
            //Friction
            Fixed64 maxForce = currentFrictionCoefficient * wheel.suspension.accumulatedImpulse;
            accumulatedImpulse = MathHelper.Clamp(accumulatedImpulse, maxForce, -maxForce);
            lambda = accumulatedImpulse - previousAccumulatedImpulse;


            //Apply the impulse
#if !WINDOWS
            FixedV3 linear = new FixedV3();
            FixedV3 angular = new FixedV3();
#else
            Vector3 linear, angular;
#endif
            linear.X = lambda * linearAX;
            linear.Y = lambda * linearAY;
            linear.Z = lambda * linearAZ;
            if (vehicleEntity.isDynamic)
            {
                angular.X = lambda * angularAX;
                angular.Y = lambda * angularAY;
                angular.Z = lambda * angularAZ;
                vehicleEntity.ApplyLinearImpulse(ref linear);
                vehicleEntity.ApplyAngularImpulse(ref angular);
            }
            if (supportIsDynamic)
            {
                linear.X = -linear.X;
                linear.Y = -linear.Y;
                linear.Z = -linear.Z;
                angular.X = lambda * angularBX;
                angular.Y = lambda * angularBY;
                angular.Z = lambda * angularBZ;
                supportEntity.ApplyLinearImpulse(ref linear);
                supportEntity.ApplyAngularImpulse(ref angular);
            }

            return lambda;
        }

        internal void PreStep(Fixed64 dt)
        {
            vehicleEntity = wheel.Vehicle.Body;
            supportEntity = wheel.SupportingEntity;
            supportIsDynamic = supportEntity != null && supportEntity.isDynamic;

            FixedV3.Cross(ref wheel.normal, ref wheel.slidingFriction.slidingFrictionAxis, out forceAxis);
            forceAxis.Normalize();
            //Do not need to check for normalize safety because normal and sliding friction axis must be perpendicular.

            linearAX = forceAxis.X;
            linearAY = forceAxis.Y;
            linearAZ = forceAxis.Z;

            //angular A = Ra x N
            angularAX = (wheel.ra.Y * linearAZ) - (wheel.ra.Z * linearAY);
            angularAY = (wheel.ra.Z * linearAX) - (wheel.ra.X * linearAZ);
            angularAZ = (wheel.ra.X * linearAY) - (wheel.ra.Y * linearAX);

            //Angular B = N x Rb
            angularBX = (linearAY * wheel.rb.Z) - (linearAZ * wheel.rb.Y);
            angularBY = (linearAZ * wheel.rb.X) - (linearAX * wheel.rb.Z);
            angularBZ = (linearAX * wheel.rb.Y) - (linearAY * wheel.rb.X);

            //Compute inverse effective mass matrix
            Fixed64 entryA, entryB;

            //these are the transformed coordinates
            Fixed64 tX, tY, tZ;
            if (vehicleEntity.isDynamic)
            {
                tX = angularAX * vehicleEntity.inertiaTensorInverse.M11 + angularAY * vehicleEntity.inertiaTensorInverse.M21 + angularAZ * vehicleEntity.inertiaTensorInverse.M31;
                tY = angularAX * vehicleEntity.inertiaTensorInverse.M12 + angularAY * vehicleEntity.inertiaTensorInverse.M22 + angularAZ * vehicleEntity.inertiaTensorInverse.M32;
                tZ = angularAX * vehicleEntity.inertiaTensorInverse.M13 + angularAY * vehicleEntity.inertiaTensorInverse.M23 + angularAZ * vehicleEntity.inertiaTensorInverse.M33;
                entryA = tX * angularAX + tY * angularAY + tZ * angularAZ + vehicleEntity.inverseMass;
            }
            else
                entryA = F64.C0;

            if (supportIsDynamic)
            {
                tX = angularBX * supportEntity.inertiaTensorInverse.M11 + angularBY * supportEntity.inertiaTensorInverse.M21 + angularBZ * supportEntity.inertiaTensorInverse.M31;
                tY = angularBX * supportEntity.inertiaTensorInverse.M12 + angularBY * supportEntity.inertiaTensorInverse.M22 + angularBZ * supportEntity.inertiaTensorInverse.M32;
                tZ = angularBX * supportEntity.inertiaTensorInverse.M13 + angularBY * supportEntity.inertiaTensorInverse.M23 + angularBZ * supportEntity.inertiaTensorInverse.M33;
                entryB = tX * angularBX + tY * angularBY + tZ * angularBZ + supportEntity.inverseMass;
            }
            else
                entryB = F64.C0;

            velocityToImpulse = -1 / (entryA + entryB); //Softness?

            currentFrictionCoefficient = gripFrictionBlender(gripFriction, wheel.supportMaterial.kineticFriction, true, wheel);

            //Compute the maximum force
            if (targetSpeed > F64.C0)
                maxMotorForceDt = maximumForwardForce * dt;
            else
                maxMotorForceDt = -maximumBackwardForce * dt;




        }

        internal void ExclusiveUpdate()
        {
            //Warm starting
#if !WINDOWS
            FixedV3 linear = new FixedV3();
            FixedV3 angular = new FixedV3();
#else
            Vector3 linear, angular;
#endif
            linear.X = accumulatedImpulse * linearAX;
            linear.Y = accumulatedImpulse * linearAY;
            linear.Z = accumulatedImpulse * linearAZ;
            if (vehicleEntity.isDynamic)
            {
                angular.X = accumulatedImpulse * angularAX;
                angular.Y = accumulatedImpulse * angularAY;
                angular.Z = accumulatedImpulse * angularAZ;
                vehicleEntity.ApplyLinearImpulse(ref linear);
                vehicleEntity.ApplyAngularImpulse(ref angular);
            }
            if (supportIsDynamic)
            {
                linear.X = -linear.X;
                linear.Y = -linear.Y;
                linear.Z = -linear.Z;
                angular.X = accumulatedImpulse * angularBX;
                angular.Y = accumulatedImpulse * angularBY;
                angular.Z = accumulatedImpulse * angularBZ;
                supportEntity.ApplyLinearImpulse(ref linear);
                supportEntity.ApplyAngularImpulse(ref angular);
            }
        }
    }
}
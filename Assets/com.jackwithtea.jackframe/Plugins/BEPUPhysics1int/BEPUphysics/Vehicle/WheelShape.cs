﻿using System;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Materials;
using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Vehicle
{
    /// <summary>
    /// Superclass for the shape of the tires of a vehicle.
    /// Responsible for figuring out where the wheel touches the ground and
    /// managing graphical properties.
    /// </summary>
    public abstract class WheelShape : ICollisionRulesOwner
    {
        private Fixed64 airborneWheelAcceleration = (Fixed64)40;


        private Fixed64 airborneWheelDeceleration = (Fixed64)4;
        private Fixed64 brakeFreezeWheelDeceleration = (Fixed64)40;

        /// <summary>
        /// Collects collision pairs from the environment.
        /// </summary>
        protected internal Box detector = new Box(FixedV3.Zero, F64.C0, F64.C0, F64.C0);

        protected internal BEPUMatrix localGraphicTransform;
        protected Fixed64 spinAngle;


        protected Fixed64 spinVelocity;
        internal Fixed64 steeringAngle;

        internal BEPUMatrix steeringTransform;
        protected internal Wheel wheel;

        protected internal BEPUMatrix worldTransform;

        CollisionRules collisionRules = new CollisionRules() { Group = CollisionRules.DefaultDynamicCollisionGroup };
        /// <summary>
        /// Gets or sets the collision rules used by the wheel.
        /// </summary>
        public CollisionRules CollisionRules
        {
            get { return collisionRules; }
            set { collisionRules = value; }
        }

        /// <summary>
        /// Gets or sets the graphical radius of the wheel.
        /// </summary>
        public abstract Fixed64 Radius { get; set; }

        /// <summary>
        /// Gets or sets the rate at which the wheel's spinning velocity increases when accelerating and airborne.
        /// This is a purely graphical effect.
        /// </summary>
        public Fixed64 AirborneWheelAcceleration
        {
            get { return airborneWheelAcceleration; }
            set { airborneWheelAcceleration = Fixed64.Abs(value); }
        }

        /// <summary>
        /// Gets or sets the rate at which the wheel's spinning velocity decreases when the wheel is airborne and its motor is idle.
        /// This is a purely graphical effect.
        /// </summary>
        public Fixed64 AirborneWheelDeceleration
        {
            get { return airborneWheelDeceleration; }
            set { airborneWheelDeceleration = Fixed64.Abs(value); }
        }

        /// <summary>
        /// Gets or sets the rate at which the wheel's spinning velocity decreases when braking.
        /// This is a purely graphical effect.
        /// </summary>
        public Fixed64 BrakeFreezeWheelDeceleration
        {
            get { return brakeFreezeWheelDeceleration; }
            set { brakeFreezeWheelDeceleration = Fixed64.Abs(value); }
        }

        /// <summary>
        /// Gets the detector entity used by the wheelshape to collect collision pairs.
        /// </summary>
        public Box Detector
        {
            get { return detector; }
        }

        /// <summary>
        /// Gets or sets whether or not to halt the wheel spin while the WheelBrake is active.
        /// </summary>
        public bool FreezeWheelsWhileBraking { get; set; }

        /// <summary>
        /// Gets or sets the local graphic transform of the wheel shape.
        /// This transform is applied first when creating the shape's worldTransform.
        /// </summary>
        public BEPUMatrix LocalGraphicTransform
        {
            get { return localGraphicTransform; }
            set { localGraphicTransform = value; }
        }

        /// <summary>
        /// Gets or sets the current spin angle of this wheel.
        /// This changes each frame based on the relative velocity between the
        /// support and the wheel.
        /// </summary>
        public Fixed64 SpinAngle
        {
            get { return spinAngle; }
            set { spinAngle = value; }
        }

        /// <summary>
        /// Gets or sets the graphical spin velocity of the wheel based on the relative velocity 
        /// between the support and the wheel.  Whenever the wheel is in contact with
        /// the ground, the spin velocity will be each frame.
        /// </summary>
        public Fixed64 SpinVelocity
        {
            get { return spinVelocity; }
            set { spinVelocity = value; }
        }

        /// <summary>
        /// Gets or sets the current steering angle of this wheel.
        /// </summary>
        public Fixed64 SteeringAngle
        {
            get { return steeringAngle; }
            set { steeringAngle = value; }
        }

        /// <summary>
        /// Gets the wheel object associated with this shape.
        /// </summary>
        public Wheel Wheel
        {
            get { return wheel; }
            internal set { wheel = value; }
        }

        /// <summary>
        /// Gets the world matrix of the wheel for positioning a graphic.
        /// </summary>
        public BEPUMatrix WorldTransform
        {
            get { return worldTransform; }
        }


        /// <summary>
        /// Updates the wheel's world transform for graphics.
        /// Called automatically by the owning wheel at the end of each frame.
        /// If the engine is updating asynchronously, you can call this inside of a space read buffer lock
        /// and update the wheel transforms safely.
        /// </summary>
        public abstract void UpdateWorldTransform();


        internal void OnAdditionToSpace(BEPUSpace space)
        {
            detector.CollisionInformation.collisionRules.Specific.Add(wheel.vehicle.Body.CollisionInformation.collisionRules, CollisionRule.NoBroadPhase);
            detector.CollisionInformation.collisionRules.Personal = CollisionRule.NoNarrowPhaseUpdate;
            detector.CollisionInformation.collisionRules.group = CollisionRules.DefaultDynamicCollisionGroup;
            //Need to put the detectors in appropriate locations before adding, or else the broad phase would see objects at (0,0,0) and make things gross.
            UpdateDetectorPosition();
            space.Add(detector);

        }

        internal void OnRemovalFromSpace(BEPUSpace space)
        {
            space.Remove(detector);
            detector.CollisionInformation.CollisionRules.Specific.Remove(wheel.vehicle.Body.CollisionInformation.collisionRules);
        }

        /// <summary>
        /// Updates the spin velocity and spin angle for the shape.
        /// </summary>
        /// <param name="dt">Simulation timestep.</param>
        internal void UpdateSpin(Fixed64 dt)
        {
            if (wheel.HasSupport && !(wheel.brake.IsBraking && FreezeWheelsWhileBraking))
            {
                //On the ground, not braking.
                spinVelocity = wheel.drivingMotor.RelativeVelocity / Radius;
            }
            else if (wheel.HasSupport && wheel.brake.IsBraking && FreezeWheelsWhileBraking)
            {
                //On the ground, braking
                Fixed64 deceleratedValue = F64.C0;
                if (spinVelocity > F64.C0)
                    deceleratedValue = MathHelper.Max(spinVelocity - brakeFreezeWheelDeceleration * dt, F64.C0);
                else if (spinVelocity < F64.C0)
                    deceleratedValue = MathHelper.Min(spinVelocity + brakeFreezeWheelDeceleration * dt, F64.C0);

                spinVelocity = wheel.drivingMotor.RelativeVelocity / Radius;

                if (Fixed64.Abs(deceleratedValue) < Fixed64.Abs(spinVelocity))
                    spinVelocity = deceleratedValue;
            }
            else if (!wheel.HasSupport && wheel.drivingMotor.TargetSpeed != F64.C0)
            {
                //Airborne and accelerating, increase spin velocity.
                Fixed64 maxSpeed = Fixed64.Abs(wheel.drivingMotor.TargetSpeed) / Radius;
                spinVelocity = MathHelper.Clamp(spinVelocity + Fixed64.Sign(wheel.drivingMotor.TargetSpeed) * airborneWheelAcceleration * dt, -maxSpeed, maxSpeed);
            }
            else if (!wheel.HasSupport && wheel.Brake.IsBraking)
            {
                //Airborne and braking
                if (spinVelocity > F64.C0)
                    spinVelocity = MathHelper.Max(spinVelocity - brakeFreezeWheelDeceleration * dt, F64.C0);
                else if (spinVelocity < F64.C0)
                    spinVelocity = MathHelper.Min(spinVelocity + brakeFreezeWheelDeceleration * dt, F64.C0);
            }
            else if (!wheel.HasSupport)
            {
                //Just idly slowing down.
                if (spinVelocity > F64.C0)
                    spinVelocity = MathHelper.Max(spinVelocity - airborneWheelDeceleration * dt, F64.C0);
                else if (spinVelocity < F64.C0)
                    spinVelocity = MathHelper.Min(spinVelocity + airborneWheelDeceleration * dt, F64.C0);
            }
            spinAngle += spinVelocity * dt;
        }

        /// <summary>
        /// Finds a supporting entity, the contact location, and the contact normal.
        /// </summary>
        /// <param name="location">Contact point between the wheel and the support.</param>
        /// <param name="normal">Contact normal between the wheel and the support.</param>
        /// <param name="suspensionLength">Length of the suspension at the contact.</param>
        /// <param name="supportCollidable">Collidable supporting the wheel, if any.</param>
        /// <param name="entity">Entity supporting the wheel, if any.</param>
        /// <param name="material">Material of the support.</param>
        /// <returns>Whether or not any support was found.</returns>
        protected internal abstract bool FindSupport(out FixedV3 location, out FixedV3 normal, out Fixed64 suspensionLength, out Collidable supportCollidable, out Entity entity, out Material material);

        /// <summary>
        /// Initializes the detector entity and any other necessary logic.
        /// </summary>
        protected internal abstract void Initialize();

        /// <summary>
        /// Updates the position of the detector before each step.
        /// </summary>
        protected internal abstract void UpdateDetectorPosition();

    }
}
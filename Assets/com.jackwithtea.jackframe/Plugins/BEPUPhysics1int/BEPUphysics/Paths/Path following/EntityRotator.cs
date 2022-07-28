using System;
using BEPUPhysics1int.Constraints.SingleEntity;
using BEPUPhysics1int.Constraints.TwoEntity.Motors;
using BEPUPhysics1int.UpdateableSystems;
using FixMath.NET;

namespace BEPUPhysics1int.Paths.PathFollowing
{
    /// <summary>
    /// Changes the angular velocity of an entity to reach goal orientations.
    /// </summary>
    public class EntityRotator : Updateable, IDuringForcesUpdateable
    {
        private Entity entity;

        /// <summary>
        /// Constructs a new EntityRotator.
        /// </summary>
        /// <param name="e">Entity to move.</param>
        public EntityRotator(Entity e)
        {
            IsUpdatedSequentially = false;
            AngularMotor = new SingleEntityAngularMotor(e);
            Entity = e;

            AngularMotor.Settings.Mode = MotorMode.Servomechanism;
            TargetOrientation = e.Orientation;
        }

        /// <summary>
        /// Constructs a new EntityRotator.
        /// </summary>
        /// <param name="e">Entity to move.</param>
        /// <param name="angularMotor">Motor to use for angular motion if the entity is dynamic.</param>
        public EntityRotator(Entity e, SingleEntityAngularMotor angularMotor)
        {
            IsUpdatedSequentially = false;
            AngularMotor = angularMotor;
            Entity = e;

            angularMotor.Entity = Entity;
            angularMotor.Settings.Mode = MotorMode.Servomechanism;
            TargetOrientation = e.Orientation;
        }


        /// <summary>
        /// Gets the angular motor used by the entity rotator.
        /// When the affected entity is dynamic, it is pushed by motors.
        /// This ensures that its interactions and collisions with
        /// other entities remain stable.
        /// </summary>
        public SingleEntityAngularMotor AngularMotor { get; private set; }

        /// <summary>
        /// Gets or sets the entity being pushed by the entity rotator.
        /// </summary>
        public Entity Entity
        {
            get { return entity; }
            set
            {
                entity = value;
                AngularMotor.Entity = value;
            }
        }

        /// <summary>
        /// Gets or sets the target orientation of the entity rotator.
        /// </summary>
        public FixedQuaternion TargetOrientation { get; set; }

        /// <summary>
        /// Gets the angular velocity necessary to change an entity's orientation from
        /// the starting quaternion to the ending quaternion over time dt.
        /// </summary>
        /// <param name="start">Initial orientation.</param>
        /// <param name="end">Final orientation.</param>
        /// <param name="dt">Time over which the angular velocity is to be applied.</param>
        /// <returns>Angular velocity to reach the goal in time.</returns>
        public static FixedV3 GetAngularVelocity(FixedQuaternion start, FixedQuaternion end, Fixed64 dt)
        {
            //Compute the relative orientation R' between R and the target relative orientation.
            FixedQuaternion errorOrientation;
            FixedQuaternion.Conjugate(ref start, out errorOrientation);
            FixedQuaternion.Multiply(ref end, ref errorOrientation, out errorOrientation);

            FixedV3 axis;
			Fixed64 angle;
            //Turn this into an axis-angle representation.
            FixedQuaternion.GetAxisAngleFromQuaternion(ref errorOrientation, out axis, out angle);
            FixedV3.Multiply(ref axis, angle / dt, out axis);
            return axis;
        }

        /// <summary>
        /// Adds the motors to the solver.  Called automatically.
        /// </summary>
        public override void OnAdditionToSpace(BEPUSpace newSpace)
        {
            newSpace.Add(AngularMotor);
        }

        /// <summary>
        /// Removes the motors from the solver.  Called automatically.
        /// </summary>
        public override void OnRemovalFromSpace(BEPUSpace oldSpace)
        {
            oldSpace.Remove(AngularMotor);
        }

        /// <summary>
        /// Called automatically by the space.
        /// </summary>
        /// <param name="dt">Simulation timestep.</param>
        void IDuringForcesUpdateable.Update(Fixed64 dt)
        {
            if (Entity != AngularMotor.Entity)
                throw new InvalidOperationException(
                    "EntityRotator's entity differs from EntityRotator's motor's entities.  Ensure that the moved entity is only changed by setting the EntityRotator's entity property.");

            if (Entity.IsDynamic)
            {
                AngularMotor.IsActive = true;
                AngularMotor.Settings.Servo.Goal = TargetOrientation;
            }
            else
            {
                AngularMotor.IsActive = false;
                Entity.AngularVelocity = GetAngularVelocity(Entity.Orientation, TargetOrientation, dt);
            }
        }
    }
}
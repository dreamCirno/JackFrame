﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using BEPUPhysics1int.BroadPhaseEntries;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;
using BEPUPhysics1int.UpdateableSystems;
using BEPUPhysics1int.NarrowPhaseSystems.Pairs;
using BEPUPhysics1int.Materials;
using BEPUPhysics1int.PositionUpdating;
using System.Threading;
using FixMath.NET;

namespace BEPUPhysics1int.Character
{
    /// <summary>
    /// Gives a physical object simple and cheap FPS-like control.
    /// This character has less features than the full CharacterController but is a little bit faster.
    /// </summary>
    public class SphereCharacterController : Updateable, IBeforeSolverUpdateable
    {
        /// <summary>
        /// Gets the physical body of the character.
        /// </summary>
        public Sphere Body { get; private set; }

        /// <summary>
        /// Gets the contact categorizer used by the character to determine how contacts affect the character's movement.
        /// </summary>
        public CharacterContactCategorizer ContactCategorizer { get; private set; }

        /// <summary>
        /// Gets the support system which other systems use to perform local ray casts and contact queries.
        /// </summary>
        public QueryManager QueryManager { get; private set; }

        /// <summary>
        /// Gets the constraint used by the character to handle horizontal motion.  This includes acceleration due to player input and deceleration when the relative velocity
        /// between the support and the character exceeds specified maximums.
        /// </summary>
        public HorizontalMotionConstraint HorizontalMotionConstraint { get; private set; }

        /// <summary>
        /// Gets the constraint used by the character to stay glued to surfaces it stands on.
        /// </summary>
        public VerticalMotionConstraint VerticalMotionConstraint { get; private set; }

        /// <summary>
        /// Gets or sets the pair locker used by the character controller to avoid interfering with the behavior of other characters.
        /// </summary>
        private CharacterPairLocker PairLocker { get; set; }

        private FixedV3 down = new FixedV3(F64.C0, -1, F64.C0);
        /// <summary>
        /// Gets or sets the down direction of the character. Controls the interpretation of movement and support finding.
        /// </summary>
        public FixedV3 Down
        {
            get
            {
                return down;
            }
            set
            {
                Fixed64 lengthSquared = value.LengthSquared();
                if (lengthSquared < Toolbox.Epsilon)
                    return; //Silently fail. Assuming here that a dynamic process is setting this property; don't need to make a stink about it.
                FixedV3.Divide(ref value, Fixed64.Sqrt(lengthSquared), out value);
                down = value;
            }
        }

        FixedV3 viewDirection = new FixedV3(F64.C0, F64.C0, -1);

        /// <summary>
        /// Gets or sets the view direction associated with the character.
        /// Also sets the horizontal view direction internally based on the current down vector.
        /// This is used to interpret the movement directions.
        /// </summary>
        public FixedV3 ViewDirection
        {
            get
            {
                return viewDirection;
            }
            set
            {
                viewDirection = value;
            }
        }

        private Fixed64 jumpSpeed;
        /// <summary>
        /// Gets or sets the speed at which the character leaves the ground when it jumps.
        /// </summary>
        public Fixed64 JumpSpeed
        {
            get
            {
                return jumpSpeed;
            }
            set
            {
                if (value < F64.C0)
                    throw new ArgumentException("Value must be nonnegative.");
                jumpSpeed = value;
            }
        }
        Fixed64 slidingJumpSpeed;
        /// <summary>
        /// Gets or sets the speed at which the character leaves the ground when it jumps without traction.
        /// </summary>
        public Fixed64 SlidingJumpSpeed
        {
            get
            {
                return slidingJumpSpeed;
            }
            set
            {
                if (value < F64.C0)
                    throw new ArgumentException("Value must be nonnegative.");
                slidingJumpSpeed = value;
            }
        }
        Fixed64 jumpForceFactor = F64.C1;
        /// <summary>
        /// Gets or sets the amount of force to apply to supporting dynamic entities as a fraction of the force used to reach the jump speed.
        /// </summary>
        public Fixed64 JumpForceFactor
        {
            get
            {
                return jumpForceFactor;
            }
            set
            {
                if (value < F64.C0)
                    throw new ArgumentException("Value must be nonnegative.");
                jumpForceFactor = value;
            }
        }

        Fixed64 speed;
        /// <summary>
        /// Gets or sets the speed at which the character will try to move while standing with a support that provides traction.
        /// Relative velocities with a greater magnitude will be decelerated.
        /// </summary>
        public Fixed64 Speed
        {
            get
            {
                return speed;
            }
            set
            {
                if (value < F64.C0)
                    throw new ArgumentException("Value must be nonnegative.");
                speed = value;
            }
        }
        Fixed64 tractionForce;
        /// <summary>
        /// Gets or sets the maximum force that the character can apply while on a support which provides traction.
        /// </summary>
        public Fixed64 TractionForce
        {
            get
            {
                return tractionForce;
            }
            set
            {
                if (value < F64.C0)
                    throw new ArgumentException("Value must be nonnegative.");
                tractionForce = value;
            }
        }

        Fixed64 slidingSpeed;
        /// <summary>
        /// Gets or sets the speed at which the character will try to move while on a support that does not provide traction.
        /// Relative velocities with a greater magnitude will be decelerated.
        /// </summary>
        public Fixed64 SlidingSpeed
        {
            get
            {
                return slidingSpeed;
            }
            set
            {
                if (value < F64.C0)
                    throw new ArgumentException("Value must be nonnegative.");
                slidingSpeed = value;
            }
        }
        Fixed64 slidingForce;
        /// <summary>
        /// Gets or sets the maximum force that the character can apply while on a support which does not provide traction.
        /// </summary>
        public Fixed64 SlidingForce
        {
            get
            {
                return slidingForce;
            }
            set
            {
                if (value < F64.C0)
                    throw new ArgumentException("Value must be nonnegative.");
                slidingForce = value;
            }
        }

        Fixed64 airSpeed;
        /// <summary>
        /// Gets or sets the speed at which the character will try to move with no support.
        /// The character will not be decelerated while airborne.
        /// </summary>
        public Fixed64 AirSpeed
        {
            get
            {
                return airSpeed;
            }
            set
            {
                if (value < F64.C0)
                    throw new ArgumentException("Value must be nonnegative.");
                airSpeed = value;
            }
        }
        Fixed64 airForce;
        /// <summary>
        /// Gets or sets the maximum force that the character can apply with no support.
        /// </summary>
        public Fixed64 AirForce
        {
            get
            {
                return airForce;
            }
            set
            {
                if (value < F64.C0)
                    throw new ArgumentException("Value must be nonnegative.");
                airForce = value;
            }
        }

        private Fixed64 speedScale = F64.C1;
        /// <summary>
        /// Gets or sets a scaling factor to apply to the maximum speed of the character.
        /// This is useful when a character does not have 0 or MaximumSpeed target speed, but rather
        /// intermediate values. A common use case is analog controller sticks.
        /// </summary>
        public Fixed64 SpeedScale
        {
            get { return speedScale; }
            set { speedScale = value; }
        }


        /// <summary>
        /// Gets the support finder used by the character.
        /// The support finder analyzes the character's contacts to see if any of them provide support and/or traction.
        /// </summary>
        public SupportFinder SupportFinder { get; private set; }



        /// <summary>
        /// Constructs a new character controller.
        /// </summary>
        /// <param name="position">Initial position of the character.</param>
        /// <param name="radius">Radius of the character body.</param>
        /// <param name="mass">Mass of the character body.</param>
        /// <param name="maximumTractionSlope">Steepest slope, in radians, that the character can maintain traction on.</param>
        /// <param name="maximumSupportSlope">Steepest slope, in radians, that the character can consider a support.</param>
        /// <param name="speed">Speed at which the character will try to move while crouching with a support that provides traction.
        /// Relative velocities with a greater magnitude will be decelerated.</param>
        /// <param name="tractionForce">Maximum force that the character can apply while on a support which provides traction.</param>
        /// <param name="slidingSpeed">Speed at which the character will try to move while on a support that does not provide traction.
        /// Relative velocities with a greater magnitude will be decelerated.</param>
        /// <param name="slidingForce">Maximum force that the character can apply while on a support which does not provide traction</param>
        /// <param name="airSpeed">Speed at which the character will try to move with no support.
        /// The character will not be decelerated while airborne.</param>
        /// <param name="airForce">Maximum force that the character can apply with no support.</param>
        /// <param name="jumpSpeed">Speed at which the character leaves the ground when it jumps</param>
        /// <param name="slidingJumpSpeed">Speed at which the character leaves the ground when it jumps without traction</param>
        /// <param name="maximumGlueForce">Maximum force the vertical motion constraint is allowed to apply in an attempt to keep the character on the ground.</param>
        public SphereCharacterController(
			// Fix64 cannot be used for default parameters. As a workaround, make all parameters nullable and assign default values inside the constructor
			FixedV3 position = new FixedV3(),
            Fixed64? radius = null, Fixed64? mass = null,
            Fixed64? maximumTractionSlope = null, Fixed64? maximumSupportSlope = null,
            Fixed64? speed = null, Fixed64? tractionForce = null, Fixed64? slidingSpeed = null, Fixed64? slidingForce = null, Fixed64? airSpeed = null, Fixed64? airForce = null,
            Fixed64? jumpSpeed = null, Fixed64? slidingJumpSpeed = null,
            Fixed64? maximumGlueForce = null)
        {
			if (radius == null)
				radius = (Fixed64).85m;
			if (mass == null)
				mass = 10;
			if (maximumTractionSlope == null)
				maximumTractionSlope = (Fixed64)0.8m;
			if (maximumSupportSlope == null)
				maximumSupportSlope = (Fixed64)1.3m;
			if (speed == null)
				speed = 8;
			if (tractionForce == null)
				tractionForce = 1000;
			if (slidingSpeed == null)
				slidingSpeed = 6;
			if (slidingForce == null)
				slidingForce = 50;
			if (airSpeed == null)
				airSpeed = 1;
			if (airForce == null)
				airForce = 250;
			if (jumpSpeed == 0)
				jumpSpeed = (Fixed64)4.5m;
			if (slidingJumpSpeed == null)
				slidingJumpSpeed = 3;
			if (maximumGlueForce == null)
				maximumGlueForce = 5000;

			Body = new Sphere(position, (Fixed64)radius, (Fixed64)mass);
            Body.IgnoreShapeChanges = true; //Wouldn't want inertia tensor recomputations to occur if the shape changes.
            //Making the character a continuous object prevents it from flying through walls which would be pretty jarring from a player's perspective.
            Body.PositionUpdateMode = PositionUpdateMode.Continuous;
            Body.LocalInertiaTensorInverse = new BEPUMatrix3x3();
            //TODO: In v0.16.2, compound bodies would override the material properties that get set in the CreatingPair event handler.
            //In a future version where this is changed, change this to conceptually minimally required CreatingPair.
            Body.CollisionInformation.Events.DetectingInitialCollision += RemoveFriction;
            Body.LinearDamping = F64.C0;
            ContactCategorizer = new CharacterContactCategorizer((Fixed64)maximumTractionSlope, (Fixed64)maximumSupportSlope);
            QueryManager = new QueryManager(Body, ContactCategorizer);
            SupportFinder = new SupportFinder(Body, QueryManager, ContactCategorizer);
            HorizontalMotionConstraint = new HorizontalMotionConstraint(Body, SupportFinder);
            HorizontalMotionConstraint.PositionAnchorDistanceThreshold = (Fixed64)(3m / 17m) * (Fixed64)radius;
            VerticalMotionConstraint = new VerticalMotionConstraint(Body, SupportFinder, (Fixed64)maximumGlueForce);
            PairLocker = new CharacterPairLocker(Body);

            Speed = (Fixed64)speed;
            TractionForce = (Fixed64)tractionForce;
            SlidingSpeed = (Fixed64)slidingSpeed;
            SlidingForce = (Fixed64)slidingForce;
            AirSpeed = (Fixed64)airSpeed;
            AirForce = (Fixed64)airForce;
            JumpSpeed = (Fixed64)jumpSpeed;
            SlidingJumpSpeed = (Fixed64)slidingJumpSpeed;

            //Enable multithreading for the sphere characters.  
            //See the bottom of the Update method for more information about using multithreading with this character.
            IsUpdatedSequentially = false;

            //Link the character body to the character controller so that it can be identified by the locker.
            //Any object which replaces this must implement the ICharacterTag for locking to work properly.
            Body.CollisionInformation.Tag = new CharacterSynchronizer(Body);



        }

        void RemoveFriction(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair)
        {
            var collidablePair = pair as CollidablePairHandler;
            if (collidablePair != null)
            {
                //The default values for InteractionProperties is all zeroes- zero friction, zero bounciness.
                //That's exactly how we want the character to behave when hitting objects.
                collidablePair.UpdateMaterialProperties(new InteractionProperties());
            }
        }

        void ExpandBoundingBox()
        {
            if (Body.ActivityInformation.IsActive)
            {
                //This runs after the bounding box updater is run, but before the broad phase.
                //The expansion allows the downward pointing raycast to collect hit points.
                FixedV3 expansion = SupportFinder.MaximumAssistedDownStepHeight * down;
                BoundingBox box = Body.CollisionInformation.BoundingBox;
                if (down.X < F64.C0)
                    box.Min.X += expansion.X;
                else
                    box.Max.X += expansion.X;
                if (down.Y < F64.C0)
                    box.Min.Y += expansion.Y;
                else
                    box.Max.Y += expansion.Y;
                if (down.Z < F64.C0)
                    box.Min.Z += expansion.Z;
                else
                    box.Max.Z += expansion.Z;
                Body.CollisionInformation.BoundingBox = box;
            }


        }

        void IBeforeSolverUpdateable.Update(Fixed64 dt)
        {
            //Someone may want to use the Body.CollisionInformation.Tag for their own purposes.
            //That could screw up the locking mechanism above and would be tricky to track down.
            //Consider using the making the custom tag implement ICharacterTag, modifying LockCharacterPairs to analyze the different Tag type, or using the Entity.Tag for the custom data instead.
            Debug.Assert(Body.CollisionInformation.Tag is ICharacterTag, "The character.Body.CollisionInformation.Tag must implement ICharacterTag to link the SphereCharacterController and its body together for character-related locking to work in multithreaded simulations.");

            SupportData supportData;

            HorizontalMotionConstraint.UpdateMovementBasis(ref viewDirection);
            //We can't let multiple characters manage the same pairs simultaneously.  Lock it up!
            PairLocker.LockCharacterPairs();
            try
            {
                bool hadSupport = SupportFinder.HasSupport;

                SupportFinder.UpdateSupports(ref HorizontalMotionConstraint.movementDirection3d);
                supportData = SupportFinder.SupportData;

                //Compute the initial velocities relative to the support.
                FixedV3 relativeVelocity;
                ComputeRelativeVelocity(ref supportData, out relativeVelocity);
                Fixed64 verticalVelocity = FixedV3.Dot(supportData.Normal, relativeVelocity);



                //Don't attempt to use an object as support if we are flying away from it (and we were never standing on it to begin with).
                if (SupportFinder.HasSupport && !hadSupport && verticalVelocity < F64.C0)
                {
                    SupportFinder.ClearSupportData();
                    supportData = new SupportData();
                }



                //Attempt to jump.
                if (tryToJump) //Jumping while crouching would be a bit silly.
                {
                    //In the following, note that the jumping velocity changes are computed such that the separating velocity is specifically achieved,
                    //rather than just adding some speed along an arbitrary direction.  This avoids some cases where the character could otherwise increase
                    //the jump speed, which may not be desired.
                    if (SupportFinder.HasTraction)
                    {
                        //The character has traction, so jump straight up.
                        Fixed64 currentDownVelocity;
                        FixedV3.Dot(ref down, ref relativeVelocity, out currentDownVelocity);
                        //Target velocity is JumpSpeed.
                        Fixed64 velocityChange = MathHelper.Max(jumpSpeed + currentDownVelocity, F64.C0);
                        ApplyJumpVelocity(ref supportData, down * -velocityChange, ref relativeVelocity);


                        //Prevent any old contacts from hanging around and coming back with a negative depth.
                        foreach (var pair in Body.CollisionInformation.Pairs)
                            pair.ClearContacts();
                        SupportFinder.ClearSupportData();
                        supportData = new SupportData();
                    }
                    else if (SupportFinder.HasSupport)
                    {
                        //The character does not have traction, so jump along the surface normal instead.
                        Fixed64 currentNormalVelocity = FixedV3.Dot(supportData.Normal, relativeVelocity);
                        //Target velocity is JumpSpeed.
                        Fixed64 velocityChange = MathHelper.Max(slidingJumpSpeed - currentNormalVelocity, F64.C0);
                        ApplyJumpVelocity(ref supportData, supportData.Normal * -velocityChange, ref relativeVelocity);

                        //Prevent any old contacts from hanging around and coming back with a negative depth.
                        foreach (var pair in Body.CollisionInformation.Pairs)
                            pair.ClearContacts();
                        SupportFinder.ClearSupportData();
                        supportData = new SupportData();
                    }
                }
                tryToJump = false;
            }
            finally
            {
                PairLocker.UnlockCharacterPairs();
            }


            //Tell the constraints to get ready to solve.
            HorizontalMotionConstraint.UpdateSupportData();
            VerticalMotionConstraint.UpdateSupportData();



            //Update the horizontal motion constraint's state.
            if (supportData.SupportObject != null)
            {
                if (SupportFinder.HasTraction)
                {
                    HorizontalMotionConstraint.MovementMode = MovementMode.Traction;
                    HorizontalMotionConstraint.TargetSpeed = speed;
                    HorizontalMotionConstraint.MaximumForce = tractionForce;
                }
                else
                {
                    HorizontalMotionConstraint.MovementMode = MovementMode.Sliding;
                    HorizontalMotionConstraint.TargetSpeed = slidingSpeed;
                    HorizontalMotionConstraint.MaximumForce = slidingForce;
                }
            }
            else
            {
                HorizontalMotionConstraint.MovementMode = MovementMode.Floating;
                HorizontalMotionConstraint.TargetSpeed = airSpeed;
                HorizontalMotionConstraint.MaximumForce = airForce;
            }
            HorizontalMotionConstraint.TargetSpeed *= SpeedScale;



        }

        void ComputeRelativeVelocity(ref SupportData supportData, out FixedV3 relativeVelocity)
        {

            //Compute the relative velocity between the body and its support, if any.
            //The relative velocity will be updated as impulses are applied.
            relativeVelocity = Body.LinearVelocity;
            if (SupportFinder.HasSupport)
            {
                //Only entities have velocity.
                var entityCollidable = supportData.SupportObject as EntityCollidable;
                if (entityCollidable != null)
                {
                    //It's possible for the support's velocity to change due to another character jumping if the support is dynamic.
                    //Don't let that happen while the character is computing a relative velocity!
                    FixedV3 entityVelocity;
                    bool locked;
                    if (locked = entityCollidable.Entity.IsDynamic)
                        entityCollidable.Entity.Locker.Enter();
                    try
                    {
                        entityVelocity = Toolbox.GetVelocityOfPoint(supportData.Position, entityCollidable.Entity.Position, entityCollidable.Entity.LinearVelocity, entityCollidable.Entity.AngularVelocity);
                    }
                    finally
                    {
                        if (locked)
                            entityCollidable.Entity.Locker.Exit();
                    }
                    FixedV3.Subtract(ref relativeVelocity, ref entityVelocity, out relativeVelocity);
                }
            }

        }

        /// <summary>
        /// Changes the relative velocity between the character and its support.
        /// </summary>
        /// <param name="supportData">Support data to use to jump.</param>
        /// <param name="velocityChange">Change to apply to the character and support relative velocity.</param>
        /// <param name="relativeVelocity">Relative velocity to update.</param>
        void ApplyJumpVelocity(ref SupportData supportData, FixedV3 velocityChange, ref FixedV3 relativeVelocity)
        {
            Body.LinearVelocity += velocityChange;
            var entityCollidable = supportData.SupportObject as EntityCollidable;
            if (entityCollidable != null)
            {
                if (entityCollidable.Entity.IsDynamic)
                {
                    FixedV3 change = velocityChange * jumpForceFactor;
                    //Multiple characters cannot attempt to modify another entity's velocity at the same time.
                    entityCollidable.Entity.Locker.Enter();
                    try
                    {
                        entityCollidable.Entity.LinearMomentum += change * -Body.Mass;
                    }
                    finally
                    {
                        entityCollidable.Entity.Locker.Exit();
                    }
                    velocityChange += change;
                }
            }

            //Update the relative velocity as well.  It's a ref parameter, so this update will be reflected in the calling scope.
            FixedV3.Add(ref relativeVelocity, ref velocityChange, out relativeVelocity);

        }



        bool tryToJump;
        /// <summary>
        /// Jumps the character off of whatever it's currently standing on.  If it has traction, it will go straight up.
        /// If it doesn't have traction, but is still supported by something, it will jump in the direction of the surface normal.
        /// </summary>
        public void Jump()
        {
            //The actual jump velocities are applied next frame.  This ensures that gravity doesn't pre-emptively slow the jump, and uses more
            //up-to-date support data.
            tryToJump = true;
        }

        public override void OnAdditionToSpace(BEPUSpace newSpace)
        {
            //Add any supplements to the space too.
            newSpace.Add(Body);
            newSpace.Add(HorizontalMotionConstraint);
            newSpace.Add(VerticalMotionConstraint);
            //This character controller requires the standard implementation of Space.
            newSpace.BoundingBoxUpdater.Finishing += ExpandBoundingBox;

            Body.AngularVelocity = new FixedV3();
            Body.LinearVelocity = new FixedV3();
        }
        public override void OnRemovalFromSpace(BEPUSpace oldSpace)
        {
            //Remove any supplements from the space too.
            oldSpace.Remove(Body);
            oldSpace.Remove(HorizontalMotionConstraint);
            oldSpace.Remove(VerticalMotionConstraint);
            //This character controller requires the standard implementation of Space.
            oldSpace.BoundingBoxUpdater.Finishing -= ExpandBoundingBox;
            SupportFinder.ClearSupportData();
            Body.AngularVelocity = new FixedV3();
            Body.LinearVelocity = new FixedV3();
        }


    }
}


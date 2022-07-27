using System;
using BEPUPhysics1int.DataStructures;
using BEPUPhysics1int.Settings;
using FixMath.NET;

namespace BEPUPhysics1int.Constraints.Collision
{
    /// <summary>
    /// Computes the friction force for a contact when central friction cannot be used.
    /// </summary>
    public class ContactFrictionConstraint : SolverUpdateable
    {
        private ContactManifoldConstraint contactManifoldConstraint;
        ///<summary>
        /// Gets the manifold constraint associated with this friction constraint.
        ///</summary>
        public ContactManifoldConstraint ContactManifoldConstraint
        {
            get
            {
                return contactManifoldConstraint;
            }
        }
        private ContactPenetrationConstraint penetrationConstraint;
        ///<summary>
        /// Gets the penetration constraint associated with this friction constraint.
        ///</summary>
        public ContactPenetrationConstraint PenetrationConstraint
        {
            get
            {
                return penetrationConstraint;
            }
        }

        ///<summary>
        /// Constructs a new friction constraint.
        ///</summary>
        public ContactFrictionConstraint()
        {
            isActive = false;
        }

        internal Fixed64 accumulatedImpulse;
        //Fix64 linearBX, linearBY, linearBZ;
        private Fixed64 angularAX, angularAY, angularAZ;
        private Fixed64 angularBX, angularBY, angularBZ;

        //Inverse effective mass matrix


        private Fixed64 friction;
        internal Fixed64 linearAX, linearAY, linearAZ;
        private Entity entityA, entityB;
        private bool entityAIsDynamic, entityBIsDynamic;
        private Fixed64 velocityToImpulse;


        ///<summary>
        /// Configures the friction constraint for a new contact.
        ///</summary>
        ///<param name="contactManifoldConstraint">Manifold to which the constraint belongs.</param>
        ///<param name="penetrationConstraint">Penetration constraint associated with this friction constraint.</param>
        public void Setup(ContactManifoldConstraint contactManifoldConstraint, ContactPenetrationConstraint penetrationConstraint)
        {
            this.contactManifoldConstraint = contactManifoldConstraint;
            this.penetrationConstraint = penetrationConstraint;
            IsActive = true;
            linearAX = F64.C0;
            linearAY = F64.C0;
            linearAZ = F64.C0;

            entityA = contactManifoldConstraint.EntityA;
            entityB = contactManifoldConstraint.EntityB;
        }

        ///<summary>
        /// Cleans upt he friction constraint.
        ///</summary>
        public void CleanUp()
        {
            accumulatedImpulse = F64.C0;
            contactManifoldConstraint = null;
            penetrationConstraint = null;
            entityA = null;
            entityB = null;
            IsActive = false;
        }

        /// <summary>
        /// Gets the direction in which the friction force acts.
        /// </summary>
        public FixedV3 FrictionDirection
        {
            get { return new FixedV3(linearAX, linearAY, linearAZ); }
        }

        /// <summary>
        /// Gets the total impulse applied by this friction constraint in the last time step.
        /// </summary>
        public Fixed64 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        ///<summary>
        /// Gets the relative velocity of the constraint.  This is the velocity along the tangent movement direction.
        ///</summary>
        public Fixed64 RelativeVelocity
        {
            get
            {
                Fixed64 velocity = F64.C0;
                if (entityA != null)
                    velocity += entityA.linearVelocity.X * linearAX + entityA.linearVelocity.Y * linearAY + entityA.linearVelocity.Z * linearAZ +
                                entityA.angularVelocity.X * angularAX + entityA.angularVelocity.Y * angularAY + entityA.angularVelocity.Z * angularAZ;
                if (entityB != null)
                    velocity += -entityB.linearVelocity.X * linearAX - entityB.linearVelocity.Y * linearAY - entityB.linearVelocity.Z * linearAZ +
                                entityB.angularVelocity.X * angularBX + entityB.angularVelocity.Y * angularBY + entityB.angularVelocity.Z * angularBZ;
                return velocity;
            }
        }


        /// <summary>
        /// Computes one iteration of the constraint to meet the solver updateable's goal.
        /// </summary>
        /// <returns>The rough applied impulse magnitude.</returns>
        public override Fixed64 SolveIteration()
        {
            //Compute relative velocity and convert to impulse
            Fixed64 lambda = RelativeVelocity * velocityToImpulse;


            //Clamp accumulated impulse
            Fixed64 previousAccumulatedImpulse = accumulatedImpulse;
            Fixed64 maxForce = friction * penetrationConstraint.accumulatedImpulse;
            accumulatedImpulse = MathHelper.Clamp(accumulatedImpulse + lambda, -maxForce, maxForce);
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
            if (entityAIsDynamic)
            {
                angular.X = lambda * angularAX;
                angular.Y = lambda * angularAY;
                angular.Z = lambda * angularAZ;
                entityA.ApplyLinearImpulse(ref linear);
                entityA.ApplyAngularImpulse(ref angular);
            }
            if (entityBIsDynamic)
            {
                linear.X = -linear.X;
                linear.Y = -linear.Y;
                linear.Z = -linear.Z;
                angular.X = lambda * angularBX;
                angular.Y = lambda * angularBY;
                angular.Z = lambda * angularBZ;
                entityB.ApplyLinearImpulse(ref linear);
                entityB.ApplyAngularImpulse(ref angular);
            }

            return Fixed64.Abs(lambda);
        }

        /// <summary>
        /// Initializes the constraint for this frame.
        /// </summary>
        /// <param name="dt">Time since the last frame.</param>
        public override void Update(Fixed64 dt)
        {


            entityAIsDynamic = entityA != null && entityA.isDynamic;
            entityBIsDynamic = entityB != null && entityB.isDynamic;

            //Compute the three dimensional relative velocity at the point.

            FixedV3 velocityA = new FixedV3(), velocityB = new FixedV3();
            FixedV3 ra = penetrationConstraint.ra, rb = penetrationConstraint.rb;
            if (entityA != null)
            {
                FixedV3.Cross(ref entityA.angularVelocity, ref ra, out velocityA);
                FixedV3.Add(ref velocityA, ref entityA.linearVelocity, out velocityA);
            }
            if (entityB != null)
            {
                FixedV3.Cross(ref entityB.angularVelocity, ref rb, out velocityB);
                FixedV3.Add(ref velocityB, ref entityB.linearVelocity, out velocityB);
            }
            FixedV3 relativeVelocity;
            FixedV3.Subtract(ref velocityA, ref velocityB, out relativeVelocity);

            //Get rid of the normal velocity.
            FixedV3 normal = penetrationConstraint.contact.Normal;
            Fixed64 normalVelocityScalar = normal.X * relativeVelocity.X + normal.Y * relativeVelocity.Y + normal.Z * relativeVelocity.Z;
            relativeVelocity.X -= normalVelocityScalar * normal.X;
            relativeVelocity.Y -= normalVelocityScalar * normal.Y;
            relativeVelocity.Z -= normalVelocityScalar * normal.Z;

            //Create the jacobian entry and decide the friction coefficient.
            Fixed64 length = relativeVelocity.LengthSquared();
            if (length > Toolbox.Epsilon)
            {
                length = Fixed64.Sqrt(length);
                linearAX = relativeVelocity.X / length;
                linearAY = relativeVelocity.Y / length;
                linearAZ = relativeVelocity.Z / length;

                friction = length > CollisionResponseSettings.StaticFrictionVelocityThreshold
                               ? contactManifoldConstraint.materialInteraction.KineticFriction
                               : contactManifoldConstraint.materialInteraction.StaticFriction;
            }
            else
            {
                //If there's no velocity, there's no jacobian.  Give up.
                //This is 'fast' in that it will early out on essentially resting objects,
                //but it may introduce instability.
                //If it doesn't look good, try the next approach.
                //isActive = false;
                //return;

                //if the above doesn't work well, try using the previous frame's jacobian.
                if (linearAX != F64.C0 || linearAY != F64.C0 || linearAZ != F64.C0)
                {
                    friction = contactManifoldConstraint.materialInteraction.StaticFriction;
                }
                else
                {
                    //Can't really do anything here, give up.
                    isActiveInSolver = false;
                    return;
                    //Could also cross the up with normal to get a random direction.  Questionable value.
                }
            }


            //angular A = Ra x N
            angularAX = (ra.Y * linearAZ) - (ra.Z * linearAY);
            angularAY = (ra.Z * linearAX) - (ra.X * linearAZ);
            angularAZ = (ra.X * linearAY) - (ra.Y * linearAX);

            //Angular B = N x Rb
            angularBX = (linearAY * rb.Z) - (linearAZ * rb.Y);
            angularBY = (linearAZ * rb.X) - (linearAX * rb.Z);
            angularBZ = (linearAX * rb.Y) - (linearAY * rb.X);

            //Compute inverse effective mass matrix
            Fixed64 entryA, entryB;

            //these are the transformed coordinates
            Fixed64 tX, tY, tZ;
            if (entityAIsDynamic)
            {
                tX = angularAX * entityA.inertiaTensorInverse.M11 + angularAY * entityA.inertiaTensorInverse.M21 + angularAZ * entityA.inertiaTensorInverse.M31;
                tY = angularAX * entityA.inertiaTensorInverse.M12 + angularAY * entityA.inertiaTensorInverse.M22 + angularAZ * entityA.inertiaTensorInverse.M32;
                tZ = angularAX * entityA.inertiaTensorInverse.M13 + angularAY * entityA.inertiaTensorInverse.M23 + angularAZ * entityA.inertiaTensorInverse.M33;
                entryA = tX * angularAX + tY * angularAY + tZ * angularAZ + entityA.inverseMass;
            }
            else
                entryA = F64.C0;

            if (entityBIsDynamic)
            {
                tX = angularBX * entityB.inertiaTensorInverse.M11 + angularBY * entityB.inertiaTensorInverse.M21 + angularBZ * entityB.inertiaTensorInverse.M31;
                tY = angularBX * entityB.inertiaTensorInverse.M12 + angularBY * entityB.inertiaTensorInverse.M22 + angularBZ * entityB.inertiaTensorInverse.M32;
                tZ = angularBX * entityB.inertiaTensorInverse.M13 + angularBY * entityB.inertiaTensorInverse.M23 + angularBZ * entityB.inertiaTensorInverse.M33;
                entryB = tX * angularBX + tY * angularBY + tZ * angularBZ + entityB.inverseMass;
            }
            else
                entryB = F64.C0;

            velocityToImpulse = -1 / (entryA + entryB); //Softness?



        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
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
            if (entityAIsDynamic)
            {
                angular.X = accumulatedImpulse * angularAX;
                angular.Y = accumulatedImpulse * angularAY;
                angular.Z = accumulatedImpulse * angularAZ;
                entityA.ApplyLinearImpulse(ref linear);
                entityA.ApplyAngularImpulse(ref angular);
            }
            if (entityBIsDynamic)
            {
                linear.X = -linear.X;
                linear.Y = -linear.Y;
                linear.Z = -linear.Z;
                angular.X = accumulatedImpulse * angularBX;
                angular.Y = accumulatedImpulse * angularBY;
                angular.Z = accumulatedImpulse * angularBZ;
                entityB.ApplyLinearImpulse(ref linear);
                entityB.ApplyAngularImpulse(ref angular);
            }
        }

        protected internal override void CollectInvolvedEntities(RawList<Entity> outputInvolvedEntities)
        {
            //This should never really have to be called.
            if (entityA != null)
                outputInvolvedEntities.Add(entityA);
            if (entityB != null)
                outputInvolvedEntities.Add(entityB);
        }
    }
}
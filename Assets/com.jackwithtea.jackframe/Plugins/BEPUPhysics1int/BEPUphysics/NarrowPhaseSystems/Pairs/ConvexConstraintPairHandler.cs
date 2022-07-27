using System;
using BEPUPhysics1int.BroadPhaseSystems;
using BEPUPhysics1int.BroadPhaseEntries;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;
using BEPUPhysics1int.CollisionTests;
using BEPUPhysics1int.CollisionTests.CollisionAlgorithms.GJK;
using BEPUPhysics1int.CollisionTests.Manifolds;
using BEPUPhysics1int.Constraints.Collision;
using BEPUPhysics1int.PositionUpdating;
using BEPUPhysics1int.Settings;

using BEPUPhysics1int.CollisionShapes.ConvexShapes;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Pair handler that manages a pair of two boxes.
    ///</summary>
    public abstract class ConvexConstraintPairHandler : ConvexPairHandler
    {
        private ConvexContactManifoldConstraint contactConstraint;


        /// <summary>
        /// Gets the contact constraint used by the pair handler.
        /// </summary>
        public override ContactManifoldConstraint ContactConstraint
        {
            get { return contactConstraint; }
        }

        protected ConvexConstraintPairHandler()
        {
            contactConstraint = new ConvexContactManifoldConstraint(this);
        }

        protected internal override void GetContactInformation(int index, out ContactInformation info)
        {
            info.Contact = ContactManifold.contacts.Elements[index];
            //Find the contact's normal force.
            Fixed64 totalNormalImpulse = F64.C0;
            info.NormalImpulse = F64.C0;
            for (int i = 0; i < contactConstraint.penetrationConstraints.Count; i++)
            {
                totalNormalImpulse += contactConstraint.penetrationConstraints.Elements[i].accumulatedImpulse;
                if (contactConstraint.penetrationConstraints.Elements[i].contact == info.Contact)
                {
                    info.NormalImpulse = contactConstraint.penetrationConstraints.Elements[i].accumulatedImpulse;
                }
            }
            //Compute friction force.  Since we are using central friction, this is 'faked.'
            Fixed64 radius;
            FixedV3.Distance(ref contactConstraint.slidingFriction.manifoldCenter, ref info.Contact.Position, out radius);
            if (totalNormalImpulse > F64.C0)
                info.FrictionImpulse = (info.NormalImpulse / totalNormalImpulse) * (contactConstraint.slidingFriction.accumulatedImpulse.Length() + contactConstraint.twistFriction.accumulatedImpulse * radius);
            else
                info.FrictionImpulse = F64.C0;
            //Compute relative velocity
            FixedV3 velocity;
            //If the pair is handling some type of query and does not actually have supporting entities, then consider the velocity contribution to be zero.
            if (EntityA != null)
            {
                FixedV3.Subtract(ref info.Contact.Position, ref EntityA.position, out velocity);
                FixedV3.Cross(ref EntityA.angularVelocity, ref velocity, out velocity);
                FixedV3.Add(ref velocity, ref EntityA.linearVelocity, out info.RelativeVelocity);
            }
            else
                info.RelativeVelocity = new FixedV3();

            if (EntityB != null)
            {
                FixedV3.Subtract(ref info.Contact.Position, ref EntityB.position, out velocity);
                FixedV3.Cross(ref EntityB.angularVelocity, ref velocity, out velocity);
                FixedV3.Add(ref velocity, ref EntityB.linearVelocity, out velocity);
                FixedV3.Subtract(ref info.RelativeVelocity, ref velocity, out info.RelativeVelocity);
            }


            info.Pair = this;

        }

    }

}

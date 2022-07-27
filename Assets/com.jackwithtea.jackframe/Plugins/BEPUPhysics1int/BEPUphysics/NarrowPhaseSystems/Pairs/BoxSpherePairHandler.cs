using System;
using BEPUPhysics1int.BroadPhaseEntries;
using BEPUPhysics1int.BroadPhaseSystems;
using BEPUPhysics1int.BroadPhaseEntries.MobileCollidables;
using BEPUPhysics1int.CollisionTests;
using BEPUPhysics1int.CollisionTests.CollisionAlgorithms.GJK;
using BEPUPhysics1int.Constraints.Collision;
using BEPUPhysics1int.PositionUpdating;
using BEPUPhysics1int.Settings;

using BEPUPhysics1int.CollisionTests.CollisionAlgorithms;
using BEPUPhysics1int.CollisionShapes.ConvexShapes;
using BEPUPhysics1int.CollisionTests.Manifolds;
using BEPUPhysics1int;

namespace BEPUPhysics1int.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a box and sphere in a collision.
    ///</summary>
    public class BoxSpherePairHandler : ConvexPairHandler
    {
        ConvexCollidable<BoxShape> box;
        ConvexCollidable<SphereShape> sphere;

        //Using a non-convex one since they have slightly lower overhead than their Convex friends when dealing with a single contact point.
        BoxSphereContactManifold contactManifold = new BoxSphereContactManifold();
        private NonConvexContactManifoldConstraint contactConstraint;

        public override Collidable CollidableA
        {
            get { return box; }
        }

        public override Collidable CollidableB
        {
            get { return sphere; }
        }

        /// <summary>
        /// Gets the contact constraint used by the pair handler.
        /// </summary>
        public override ContactManifoldConstraint ContactConstraint
        {
            get
            {
                return contactConstraint;
            }
        }
        /// <summary>
        /// Gets the contact manifold used by the pair handler.
        /// </summary>
        public override ContactManifold ContactManifold
        {
            get { return contactManifold; }
        }

        public override Entity EntityA
        {
            get { return box.entity; }
        }

        public override Entity EntityB
        {
            get { return sphere.entity; }
        }

        public BoxSpherePairHandler()
        {
            contactConstraint = new NonConvexContactManifoldConstraint(this);
        }

        ///<summary>
        /// Initializes the pair handler.
        ///</summary>
        ///<param name="entryA">First entry in the pair.</param>
        ///<param name="entryB">Second entry in the pair.</param>
        public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        {


            box = entryA as ConvexCollidable<BoxShape>;
            sphere = entryB as ConvexCollidable<SphereShape>;

            if (box == null || sphere == null)
            {
                box = entryB as ConvexCollidable<BoxShape>;
                sphere = entryA as ConvexCollidable<SphereShape>;
                if (box == null || sphere == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
                }
            }

            //Reorder the entries so that the guarantee that the normal points from A to B is satisfied.
            broadPhaseOverlap.entryA = box;
            broadPhaseOverlap.entryB = sphere;

            base.Initialize(entryA, entryB);


        }


        ///<summary>
        /// Cleans up the pair handler.
        ///</summary>
        public override void CleanUp()
        {
            base.CleanUp();

            box = null;
            sphere = null;

        }


        protected internal override void GetContactInformation(int index, out ContactInformation info)
        {
            info.Contact = ContactManifold.contacts.Elements[index];
            //Find the contact's force.
            info.FrictionImpulse = F64.C0;
            info.NormalImpulse = F64.C0;
            for (int i = 0; i < contactConstraint.frictionConstraints.Count; i++)
            {
                if (contactConstraint.frictionConstraints.Elements[i].PenetrationConstraint.contact == info.Contact)
                {
                    info.FrictionImpulse = contactConstraint.frictionConstraints.Elements[i].accumulatedImpulse;
                    info.NormalImpulse = contactConstraint.frictionConstraints.Elements[i].PenetrationConstraint.accumulatedImpulse;
                    break;
                }
            }
            //Compute relative velocity
            FixedV3 velocity;

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

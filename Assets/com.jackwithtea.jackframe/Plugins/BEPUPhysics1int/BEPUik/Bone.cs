﻿using System;
using System.Collections.Generic;
using BEPUPhysics1int;
using BEPUPhysics1int.DataStructures;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Piece of a character which is moved by constraints.
    /// </summary>
    public class Bone
    {
        internal List<IKJoint> joints = new List<IKJoint>();

        /// <summary>
        /// Gets or sets the position of the bone.
        /// </summary>
        public FixedV3 Position;

        /// <summary>
        /// Gets or sets the orientation of the bone.
        /// </summary>
        public FixedQuaternion Orientation = FixedQuaternion.Identity;

        /// <summary>
        /// The mid-iteration angular velocity associated with the bone.
        /// This is computed during the velocity subiterations and then applied to the orientation at the end of each position iteration.
        /// </summary>
        internal FixedV3 angularVelocity;

        /// <summary>
        /// The mid-iteration linear velocity associated with the bone.
        /// This is computed during the velocity subiterations and then applied to the position at the end of each position iteration.
        /// </summary>
        internal FixedV3 linearVelocity;


        internal Fixed64 inverseMass;

        /// <summary>
        /// Gets or sets the mass of the bone.
        /// High mass bones resist motion more than those of small mass.
        /// Setting the mass updates the inertia tensor of the bone.
        /// </summary>
        public Fixed64 Mass
        {
            get { return F64.C1 / inverseMass; }
            set
            {
                //Long chains could produce exceptionally small values.
                //Attempting to invert them would result in NaNs.
                //Clamp the lowest mass to 1e-7f.
                if (value > Toolbox.Epsilon)
                    inverseMass = F64.C1 / value;
                else
                    inverseMass = (Fixed64)1e7m;
                ComputeLocalInertiaTensor();
            }
        }

        internal BEPUMatrix3x3 inertiaTensorInverse;
        internal BEPUMatrix3x3 localInertiaTensorInverse;

        /// <summary>
        /// An arbitrary scaling factor is applied to the inertia tensor. This tends to improve stability.
        /// </summary>
        public static Fixed64 InertiaTensorScaling = (Fixed64)2.5m;

        /// <summary>
        /// Gets the list of joints affecting this bone.
        /// </summary>
        public ReadOnlyList<IKJoint> Joints
        {
            get { return new ReadOnlyList<IKJoint>(joints); }
        }


        /// <summary>
        /// Gets or sets whether or not this bone is pinned. Pinned bones cannot be moved by constraints.
        /// </summary>
        public bool Pinned { get; set; }

        /// <summary>
        /// Gets whether or not the bone is a member of the active set as determined by the last IK solver execution.
        /// </summary>
        public bool IsActive { get; internal set; }

        private Fixed64 radius;
        /// <summary>
        /// Gets or sets the radius of the bone.
        /// Setting the radius changes the inertia tensor of the bone.
        /// </summary>
        public Fixed64 Radius
        {
            get
            { return radius; }
            set
            {
                radius = value;
                ComputeLocalInertiaTensor();
            }
        }

        private Fixed64 halfHeight;
        /// <summary>
        /// Gets or sets the height, divided by two, of the bone.
        /// The half height extends both ways from the center position of the bone.
        /// Setting the half height changes the inertia tensor of the bone.
        /// </summary>
        public Fixed64 HalfHeight
        {
            get { return halfHeight; }
            set
            {
                halfHeight = value;
                ComputeLocalInertiaTensor();
            }
        }

        /// <summary>
        /// Gets or sets the height of the bone.
        /// Setting the height changes the inertia tensor of the bone.
        /// </summary>
        public Fixed64 Height
        {
            get { return halfHeight * F64.C2; }
            set
            {
                halfHeight = value / F64.C2;
                ComputeLocalInertiaTensor();
            }
        }

        /// <summary>
        /// Constructs a new bone.
        /// </summary>
        /// <param name="position">Initial position of the bone.</param>
        /// <param name="orientation">Initial orientation of the bone.</param>
        /// <param name="radius">Radius of the bone.</param>
        /// <param name="height">Height of the bone.</param>
        /// <param name="mass">Mass of the bone.</param>
        public Bone(FixedV3 position, FixedQuaternion orientation, Fixed64 radius, Fixed64 height, Fixed64 mass)
            :this(position, orientation, radius, height)
        {
            Mass = mass;
        }

        /// <summary>
        /// Constructs a new bone. Assumes the mass will be set later.
        /// </summary>
        /// <param name="position">Initial position of the bone.</param>
        /// <param name="orientation">Initial orientation of the bone.</param>
        /// <param name="radius">Radius of the bone.</param>
        /// <param name="height">Height of the bone.</param>
        public Bone(FixedV3 position, FixedQuaternion orientation, Fixed64 radius, Fixed64 height)
        {
            Mass = F64.C1;
            Position = position;
            Orientation = orientation;
            Radius = radius;
            Height = height;
        }


        void ComputeLocalInertiaTensor()
        {
            var localInertiaTensor = new BEPUMatrix3x3();
            var multiplier = Mass * InertiaTensorScaling;
            Fixed64 diagValue = (F64.C0p0833333333 * Height * Height + F64.C0p25 * Radius * Radius) * multiplier;
            localInertiaTensor.M11 = diagValue;
            localInertiaTensor.M22 = F64.C0p5 * Radius * Radius * multiplier;
            localInertiaTensor.M33 = diagValue;
            BEPUMatrix3x3.Invert(ref localInertiaTensor, out localInertiaTensorInverse);
        }

        /// <summary>
        /// Updates the world inertia tensor based upon the local inertia tensor and current orientation.
        /// </summary>
        internal void UpdateInertiaTensor()
        {
            //This is separate from the position update because the orientation can change outside of our iteration loop, so this has to run first.
            //Iworld^-1 = RT * Ilocal^1 * R
            BEPUMatrix3x3 orientationMatrix;
            BEPUMatrix3x3.CreateFromQuaternion(ref Orientation, out orientationMatrix);
            BEPUMatrix3x3.MultiplyTransposed(ref orientationMatrix, ref localInertiaTensorInverse, out inertiaTensorInverse);
            BEPUMatrix3x3.Multiply(ref inertiaTensorInverse, ref orientationMatrix, out inertiaTensorInverse);
        }

        /// <summary>
        /// Integrates the position and orientation of the bone forward based upon the current linear and angular velocity.
        /// </summary>
        internal void UpdatePosition()
        {
            //Update the position based on the linear velocity.
            FixedV3.Add(ref Position, ref linearVelocity, out Position);

            //Update the orientation based on the angular velocity.
            FixedV3 increment;
            FixedV3.Multiply(ref angularVelocity, F64.C0p5, out increment);
            var multiplier = new FixedQuaternion(increment.X, increment.Y, increment.Z, F64.C0);
            FixedQuaternion.Multiply(ref multiplier, ref Orientation, out multiplier);
            FixedQuaternion.Add(ref Orientation, ref multiplier, out Orientation);
            Orientation.Normalize();

            //Eliminate any latent velocity in the bone to prevent unwanted simulation feedback.
            //This is the only thing conceptually separating this "IK" solver from the regular dynamics loop in BEPUphysics.
            //(Well, that and the whole lack of collision detection...)
            linearVelocity = new FixedV3();
            angularVelocity = new FixedV3();

            //Note: Unlike a regular dynamics simulation, we do not include any 'dt' parameter in the above integration.
            //Setting the velocity to 0 every update means that no more than a single iteration's worth of velocity accumulates.
            //Since the softness of constraints already varies with the time step and bones never accelerate for more than one frame,
            //scaling the velocity for position integration actually turns out generally worse.
            //This is not a rigorously justifiable approach, but this isn't a regular dynamic simulation anyway.
        }

        internal void ApplyLinearImpulse(ref FixedV3 impulse)
        {
            FixedV3 velocityChange;
            FixedV3.Multiply(ref impulse, inverseMass, out velocityChange);
            FixedV3.Add(ref linearVelocity, ref velocityChange, out linearVelocity);
        }

        internal void ApplyAngularImpulse(ref FixedV3 impulse)
        {
            FixedV3 velocityChange;
            BEPUMatrix3x3.Transform(ref impulse, ref inertiaTensorInverse, out velocityChange);
            FixedV3.Add(ref velocityChange, ref angularVelocity, out angularVelocity);
        }

        /// <summary>
        /// Used by the per-control traversals to find stressed paths.
        /// It has to be separate from the IsActive flag because the IsActive flag is used in the same traversal
        /// to denote all visited bones (including unstressed ones).
        /// Also used in the unstressed traversals; FindCycles uses the IsActive flag and the following DistributeMass phase uses the traversed flag.
        /// </summary>
        internal bool traversed;

        /// <summary>
        /// The number of stressed paths which use this bone. A stressed path is a possible path between a pin and a control.
        /// </summary>
        internal int stressCount;

        /// <summary>
        /// The set of parents of a given bone in a traversal. This is like a list of parents; there can be multiple incoming paths and they all need to be kept handy in order to perform some traversal details.
        /// </summary>
        internal List<Bone> predecessors = new List<Bone>();

        /// <summary>
        /// True of the bone is a member of a cycle in an unstressed part of the graph or an unstressed predecessor of an unstressed cycle.
        /// Marking all the predecessors is conceptually simpler than attempting to mark the cycles in isolation.
        /// </summary>
        internal bool unstressedCycle;
        
        /// <summary>
        /// True if the bone is targeted by a control in the current stress cycle traversal that isn't the current source control.
        /// </summary>
        internal  bool targetedByOtherControl;
    }
}

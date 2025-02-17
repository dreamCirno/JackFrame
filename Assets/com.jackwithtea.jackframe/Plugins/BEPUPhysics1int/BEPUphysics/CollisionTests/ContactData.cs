﻿ 
using System;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.CollisionTests
{
    ///<summary>
    /// Contact data created by collision detection.
    ///</summary>
    public struct ContactData :IEquatable<ContactData>
    {
        /// <summary>
        /// Amount of penetration between the two objects.
        /// </summary>
        public Fixed64 PenetrationDepth;

        /// <summary>
        /// Feature-based id used to match contacts from the previous frame to their current versions.
        /// </summary>
        public int Id;

        /// <summary>
        /// Normal direction of the surface at the contact point.
        /// </summary>
        public FixedV3 Normal;

        /// <summary>
        /// Position of the contact point.
        /// </summary>
        public FixedV3 Position;

        /// <summary>
        /// Creates a ContactData from an existing Contact.
        /// </summary>
        /// <param name="contact">Contact to copy data from.</param>
        public ContactData(Contact contact)
        {
            Id = contact.Id;
            Normal = contact.Normal;
            Position = contact.Position;
            PenetrationDepth = contact.PenetrationDepth;
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Position + ", " + Normal;
        }



        public bool Equals(ContactData other)
        {
            return other.PenetrationDepth == PenetrationDepth &&
                other.Id == Id &&
                other.Normal == Normal &&
                other.Position == Position;
        }
    }
}

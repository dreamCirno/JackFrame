﻿using System;
using BEPUPhysics1int.CollisionTests.CollisionAlgorithms.GJK;

using BEPUPhysics1int.CollisionShapes.ConvexShapes;
using BEPUPhysics1int;
using BEPUPhysics1int.Settings;
using BEPUPhysics1int.DataStructures;
using FixMath.NET;

namespace BEPUPhysics1int.CollisionTests.CollisionAlgorithms
{
    ///<summary>
    /// Persistent tester that compares triangles against convex objects.
    ///</summary>
    public class TriangleConvexPairTester : TrianglePairTester
    {
        internal ConvexShape convex;

        internal CollisionState state = CollisionState.Plane;
        /// <summary>
        /// The number of updates between attempts to reset to the cheaper Plane collision state instead of more expensive GJK-based tests.
        /// This is used by the ExternalSeparated state because it only generates a boolean result.
        /// ExternalNear, which DOES generate contact information, also uses it to reduce the frequency of voronoi region tests.
        /// Deep just transitions to ExternalNear when penetration is slight rather than trying to jump to Plane.
        /// </summary>
        private const int EscapeAttemptPeriod = 10;
        int escapeAttempts;

        FixedV3 localSeparatingAxis;

        //Relies on the triangle being located in the local space of the convex object.  The convex transform is used to transform the
        //contact points back from the convex's local space into world space.
        ///<summary>
        /// Generates a contact between the triangle and convex.
        ///</summary>
        /// <param name="triangle">Triangle to test the convex against. The input triangle should be transformed into the local space of the convex.</param>
        ///<param name="contactList">Contact between the shapes, if any.</param>
        ///<returns>Whether or not the shapes are colliding.</returns>
        public override bool GenerateContactCandidates(TriangleShape triangle, out TinyStructList<ContactData> contactList)
        {
            switch (state)
            {
                case CollisionState.Plane:
                    return DoPlaneTest(triangle, out contactList);
                case CollisionState.ExternalSeparated:
                    return DoExternalSeparated(triangle, out contactList);
                case CollisionState.ExternalNear:
                    return DoExternalNear(triangle, out contactList);
                case CollisionState.Deep:
                    return DoDeepContact(triangle, out contactList);
                default:
                    contactList = new TinyStructList<ContactData>();
                    return false;
            }



        }


        private bool DoPlaneTest(TriangleShape triangle, out TinyStructList<ContactData> contactList)
        {


            //Find closest point between object and plane.
            FixedV3 reverseNormal;
            FixedV3 ab, ac;
            FixedV3.Subtract(ref triangle.vB, ref triangle.vA, out ab);
            FixedV3.Subtract(ref triangle.vC, ref triangle.vA, out ac);
            FixedV3.Cross(ref ac, ref ab, out reverseNormal);
            //Convex position dot normal is ALWAYS zero.  The thing to look at is the plane's 'd'.
            //If the distance along the normal is positive, then the convex is 'behind' that normal.
            Fixed64 dotA;
            FixedV3.Dot(ref triangle.vA, ref reverseNormal, out dotA);

            contactList = new TinyStructList<ContactData>();
            switch (triangle.sidedness)
            {
                case TriangleSidedness.DoubleSided:
                    if (dotA < F64.C0)
                    {
                        //The reverse normal is pointing towards the convex.
                        //It needs to point away from the convex so that the direction
                        //will get the proper extreme point.
                        FixedV3.Negate(ref reverseNormal, out reverseNormal);
                        dotA = -dotA;
                    }
                    break;
                case TriangleSidedness.Clockwise:
                    //if (dotA < 0)
                    //{
                    //    //The reverse normal is pointing towards the convex.
                    //    return false;
                    //}
                    break;
                case TriangleSidedness.Counterclockwise:
                    //if (dotA > 0)
                    //{
                    //    //The reverse normal is pointing away from the convex.
                    //    return false;
                    //}

                    //The reverse normal is pointing towards the convex.
                    //It needs to point away from the convex so that the direction
                    //will get the proper extreme point.
                    FixedV3.Negate(ref reverseNormal, out reverseNormal);
                    dotA = -dotA;
                    break;
            }
            FixedV3 extremePoint;
            convex.GetLocalExtremePointWithoutMargin(ref reverseNormal, out extremePoint);


            //See if the extreme point is within the face or not.
            //It might seem like the easy "depth" test should come first, since a barycentric
            //calculation takes a bit more time.  However, transferring from plane to depth is 'rare' 
            //(like all transitions), and putting this test here is logically closer to its requirements'
            //computation.

            if (GetVoronoiRegion(triangle, ref extremePoint) != VoronoiRegion.ABC)
            {
                state = CollisionState.ExternalSeparated;
                return DoExternalSeparated(triangle, out contactList);
            }



            Fixed64 dotE;
            FixedV3.Dot(ref extremePoint, ref reverseNormal, out dotE);
            Fixed64 t = (dotA - dotE) / reverseNormal.LengthSquared();



            FixedV3 offset;
            FixedV3.Multiply(ref reverseNormal, t, out offset);

            //Compare the distance from the plane to the convex object.
            Fixed64 distanceSquared = offset.LengthSquared();

            Fixed64 marginSum = triangle.collisionMargin + convex.collisionMargin;
            //TODO: Could just normalize early and avoid computing point plane before it's necessary.  
            //Exposes a sqrt but...
            if (t <= F64.C0 || distanceSquared < marginSum * marginSum)
            {
                //The convex object is in the margin of the plane.
                //All that's left is to create the contact.


                var contact = new ContactData();
                //Displacement is from A to B.  point = A + t * AB, where t = marginA / margin.
                if (marginSum > Toolbox.Epsilon) //This can be zero! It would cause a NaN is unprotected.
                    FixedV3.Multiply(ref offset, convex.collisionMargin / marginSum, out contact.Position); //t * AB
                else contact.Position = new FixedV3();
                FixedV3.Add(ref extremePoint, ref contact.Position, out contact.Position); //A + t * AB.

                Fixed64 normalLength = reverseNormal.Length();
                FixedV3.Divide(ref reverseNormal, normalLength, out contact.Normal);
                Fixed64 distance = normalLength * t;



                contact.PenetrationDepth = marginSum - distance;

                if (contact.PenetrationDepth > marginSum)
                {
                    //Check to see if the inner sphere is touching the plane.
                    //This does not override other tests; there can be more than one contact from a single triangle.

                    ContactData alternateContact;
                    if (TryInnerSphereContact(triangle, out alternateContact))// && alternateContact.PenetrationDepth > contact.PenetrationDepth)
                    {
                        contactList.Add(ref alternateContact);
                    }

                    //The convex object is stuck deep in the plane!
                    //The most problematic case for this is when
                    //an object is right on top of a cliff.
                    //The lower, vertical triangle may occasionally detect
                    //a contact with the object, but would compute an extremely
                    //deep depth if the normal plane test was used.




                    //Verify that the depth is correct by trying another approach.
                    CollisionState previousState = state;
                    state = CollisionState.ExternalNear;
                    TinyStructList<ContactData> alternateContacts;
                    if (DoExternalNear(triangle, out alternateContacts))
                    {
                        alternateContacts.Get(0, out alternateContact);
                        if (alternateContact.PenetrationDepth + F64.C0p01 < contact.PenetrationDepth) //Bias against the subtest's result, since the plane version will probably have a better position.
                        {
                            //It WAS a bad contact.
                            contactList.Add(ref alternateContact);
                            //DoDeepContact (which can be called from within DoExternalNear) can generate two contacts, but the second contact would just be an inner sphere (which we already generated).
                            //DoExternalNear can only generate one contact.  So we only need the first contact!
                            //TODO: This is a fairly fragile connection between the two stages.  Consider robustifying. (Also, the TryInnerSphereContact is done twice! This process is very rare for marginful pairs, though)
                        }
                        else
                        {
                            //Well, it really is just that deep.
                            contactList.Add(ref contact);
                            state = previousState;
                        }
                    }
                    else
                    {
                        //If the external near test finds that there was no collision at all, 
                        //just return to plane testing.  If the point turns up outside the face region
                        //next time, the system will adapt.
                        state = previousState;
                        return false;
                    }
                }
                else
                {
                    contactList.Add(ref contact);
                }
                return true;

            }
            return false;


        }




        private bool DoExternalSeparated(TriangleShape triangle, out TinyStructList<ContactData> contactList)
        {

            if (GJKToolbox.AreShapesIntersecting(convex, triangle, ref Toolbox.RigidIdentity, ref Toolbox.RigidIdentity, ref localSeparatingAxis))
            {
                state = CollisionState.ExternalNear;
                return DoExternalNear(triangle, out contactList);
            }
            TryToEscape();
            contactList = new TinyStructList<ContactData>();
            return false;
        }

        private bool DoExternalNear(TriangleShape triangle, out TinyStructList<ContactData> contactList)
        {

            FixedV3 closestA, closestB;


            //Don't bother trying to do any clever caching.  The continually transforming simplex makes it very rarely useful.
            //TODO: Initialize the simplex of the GJK method using the 'true' center of the triangle.
            //If left unmodified, the simplex that is used in GJK will just be a point at 0,0,0, which of course is at the origin.
            //This causes an instant-out, always.  Not good!
            //By giving the contributing simplex the average centroid, it has a better guess.
            FixedV3 triangleCentroid;
            FixedV3.Add(ref triangle.vA, ref triangle.vB, out triangleCentroid);
            FixedV3.Add(ref triangleCentroid, ref triangle.vC, out triangleCentroid);
            FixedV3.Multiply(ref triangleCentroid, F64.OneThird, out triangleCentroid);

            var initialSimplex = new CachedSimplex { State = SimplexState.Point, LocalSimplexB = { A = triangleCentroid } };
            if (GJKToolbox.GetClosestPoints(convex, triangle, ref Toolbox.RigidIdentity, ref Toolbox.RigidIdentity, ref initialSimplex, out closestA, out closestB))
            {
                state = CollisionState.Deep;
                return DoDeepContact(triangle, out contactList);
            }
            FixedV3 displacement;
            FixedV3.Subtract(ref closestB, ref closestA, out displacement);
            Fixed64 distanceSquared = displacement.LengthSquared();
            Fixed64 margin = convex.collisionMargin + triangle.collisionMargin;

            contactList = new TinyStructList<ContactData>();
            if (distanceSquared < margin * margin)
            {
                //Try to generate a contact.
                var contact = new ContactData();

                //Determine if the normal points in the appropriate direction given the sidedness of the triangle.
                if (triangle.sidedness != TriangleSidedness.DoubleSided)
                {
                    FixedV3 triangleNormal, ab, ac;
                    FixedV3.Subtract(ref triangle.vB, ref triangle.vA, out ab);
                    FixedV3.Subtract(ref triangle.vC, ref triangle.vA, out ac);
                    FixedV3.Cross(ref ab, ref ac, out triangleNormal);
                    Fixed64 dot;
                    FixedV3.Dot(ref triangleNormal, ref displacement, out dot);
                    if (triangle.sidedness == TriangleSidedness.Clockwise && dot > F64.C0)
                        return false;
                    if (triangle.sidedness == TriangleSidedness.Counterclockwise && dot < F64.C0)
                        return false;
                }


                //Displacement is from A to B.  point = A + t * AB, where t = marginA / margin.
                if (margin > Toolbox.Epsilon) //This can be zero! It would cause a NaN if unprotected.
                    FixedV3.Multiply(ref displacement, convex.collisionMargin / margin, out contact.Position); //t * AB
                else contact.Position = new FixedV3();
                FixedV3.Add(ref closestA, ref contact.Position, out contact.Position); //A + t * AB.



                contact.Normal = displacement;
                Fixed64 distance = Fixed64.Sqrt(distanceSquared);
                FixedV3.Divide(ref contact.Normal, distance, out contact.Normal);
                contact.PenetrationDepth = margin - distance;



                contactList.Add(ref contact);
                TryToEscape(triangle, ref contact.Position);
                return true;

            }
            //Too far to make a contact- move back to separation.
            state = CollisionState.ExternalSeparated;
            return false;
        }

        private bool DoDeepContact(TriangleShape triangle, out TinyStructList<ContactData> contactList)
        {


            //Find the origin to triangle center offset.
            FixedV3 center;
            FixedV3.Add(ref triangle.vA, ref triangle.vB, out center);
            FixedV3.Add(ref center, ref triangle.vC, out center);
            FixedV3.Multiply(ref center, F64.OneThird, out center);

            ContactData contact;

            contactList = new TinyStructList<ContactData>();

            if (MPRToolbox.AreLocalShapesOverlapping(convex, triangle, ref center, ref Toolbox.RigidIdentity))
            {

                Fixed64 dot;


                FixedV3 triangleNormal, ab, ac;
                FixedV3.Subtract(ref triangle.vB, ref triangle.vA, out ab);
                FixedV3.Subtract(ref triangle.vC, ref triangle.vA, out ac);
                FixedV3.Cross(ref ab, ref ac, out triangleNormal);
                Fixed64 lengthSquared = triangleNormal.LengthSquared();
                if (lengthSquared < Toolbox.Epsilon * F64.C0p01)
                {
                    //Degenerate triangle! That's no good.
                    //Just use the direction pointing from A to B, "B" being the triangle.  That direction is center - origin, or just center.
                    MPRToolbox.LocalSurfaceCast(convex, triangle, ref Toolbox.RigidIdentity, ref center, out contact.PenetrationDepth, out contact.Normal, out contact.Position);
                }
                else
                {
                    //Normalize the normal.
                    FixedV3.Divide(ref triangleNormal, Fixed64.Sqrt(lengthSquared), out triangleNormal);


                    //TODO: This tests all three edge axes with a full MPR raycast.  That's not really necessary; the correct edge normal should be discoverable, resulting in a single MPR raycast.

                    //Find the edge directions that will be tested with MPR.
                    FixedV3 AO, BO, CO;
                    FixedV3 AB, BC, CA;
                    FixedV3.Subtract(ref center, ref triangle.vA, out AO);
                    FixedV3.Subtract(ref center, ref triangle.vB, out BO);
                    FixedV3.Subtract(ref center, ref triangle.vC, out CO);
                    FixedV3.Subtract(ref triangle.vB, ref triangle.vA, out AB);
                    FixedV3.Subtract(ref triangle.vC, ref triangle.vB, out BC);
                    FixedV3.Subtract(ref triangle.vA, ref triangle.vC, out CA);


                    //We don't have to worry about degenerate triangles here because we've already handled that possibility above.
                    FixedV3 ABnormal, BCnormal, CAnormal;

                    //Project the center onto the edge to find the direction from the center to the edge AB.
                    FixedV3.Dot(ref AO, ref AB, out dot);
                    FixedV3.Multiply(ref AB, dot / AB.LengthSquared(), out ABnormal);
                    FixedV3.Subtract(ref AO, ref ABnormal, out ABnormal);
                    ABnormal.Normalize();

                    //Project the center onto the edge to find the direction from the center to the edge BC.
                    FixedV3.Dot(ref BO, ref BC, out dot);
                    FixedV3.Multiply(ref BC, dot / BC.LengthSquared(), out BCnormal);
                    FixedV3.Subtract(ref BO, ref BCnormal, out BCnormal);
                    BCnormal.Normalize();

                    //Project the center onto the edge to find the direction from the center to the edge BC.
                    FixedV3.Dot(ref CO, ref CA, out dot);
                    FixedV3.Multiply(ref CA, dot / CA.LengthSquared(), out CAnormal);
                    FixedV3.Subtract(ref CO, ref CAnormal, out CAnormal);
                    CAnormal.Normalize();


                    MPRToolbox.LocalSurfaceCast(convex, triangle, ref Toolbox.RigidIdentity, ref ABnormal, out contact.PenetrationDepth, out contact.Normal);
                    //Check to see if the normal is facing in the proper direction, considering that this may not be a two-sided triangle.
                    FixedV3.Dot(ref triangleNormal, ref contact.Normal, out dot);
                    if ((triangle.sidedness == TriangleSidedness.Clockwise && dot > F64.C0) || (triangle.sidedness == TriangleSidedness.Counterclockwise && dot < F64.C0))
                    {
                        //Normal was facing the wrong way.
                        //Instead of ignoring it entirely, correct the direction to as close as it can get by removing any component parallel to the triangle normal.
                        FixedV3 previousNormal = contact.Normal;
                        FixedV3.Dot(ref contact.Normal, ref triangleNormal, out dot);

                        FixedV3 p;
                        FixedV3.Multiply(ref contact.Normal, dot, out p);
                        FixedV3.Subtract(ref contact.Normal, ref p, out contact.Normal);
                        Fixed64 length = contact.Normal.LengthSquared();
                        if (length > Toolbox.Epsilon)
                        {
                            //Renormalize the corrected normal.
                            FixedV3.Divide(ref contact.Normal, Fixed64.Sqrt(length), out contact.Normal);
                            FixedV3.Dot(ref contact.Normal, ref previousNormal, out dot);
                            contact.PenetrationDepth *= dot;
                        }
                        else
                        {
                            contact.PenetrationDepth = Fixed64.MaxValue;
                            contact.Normal = new FixedV3();
                        }
                    }



                    FixedV3 candidateNormal;
                    Fixed64 candidateDepth;

                    MPRToolbox.LocalSurfaceCast(convex, triangle, ref Toolbox.RigidIdentity, ref BCnormal, out candidateDepth, out candidateNormal);
                    //Check to see if the normal is facing in the proper direction, considering that this may not be a two-sided triangle.
                    FixedV3.Dot(ref triangleNormal, ref candidateNormal, out dot);
                    if ((triangle.sidedness == TriangleSidedness.Clockwise && dot > F64.C0) || (triangle.sidedness == TriangleSidedness.Counterclockwise && dot < F64.C0))
                    {
                        //Normal was facing the wrong way.
                        //Instead of ignoring it entirely, correct the direction to as close as it can get by removing any component parallel to the triangle normal.
                        FixedV3 previousNormal = candidateNormal;
                        FixedV3.Dot(ref candidateNormal, ref triangleNormal, out dot);

                        FixedV3 p;
                        FixedV3.Multiply(ref candidateNormal, dot, out p);
                        FixedV3.Subtract(ref candidateNormal, ref p, out candidateNormal);
                        Fixed64 length = candidateNormal.LengthSquared();
                        if (length > Toolbox.Epsilon)
                        {
                            //Renormalize the corrected normal.
                            FixedV3.Divide(ref candidateNormal, Fixed64.Sqrt(length), out candidateNormal);
                            FixedV3.Dot(ref candidateNormal, ref previousNormal, out dot);
                            candidateDepth *= dot;
                        }
                        else
                        {
                            contact.PenetrationDepth = Fixed64.MaxValue;
                            contact.Normal = new FixedV3();
                        }
                    }
                    if (candidateDepth < contact.PenetrationDepth)
                    {
                        contact.Normal = candidateNormal;
                        contact.PenetrationDepth = candidateDepth;
                    }



                    MPRToolbox.LocalSurfaceCast(convex, triangle, ref Toolbox.RigidIdentity, ref CAnormal, out candidateDepth, out candidateNormal);
                    //Check to see if the normal is facing in the proper direction, considering that this may not be a two-sided triangle.
                    FixedV3.Dot(ref triangleNormal, ref candidateNormal, out dot);
                    if ((triangle.sidedness == TriangleSidedness.Clockwise && dot > F64.C0) || (triangle.sidedness == TriangleSidedness.Counterclockwise && dot < F64.C0))
                    {
                        //Normal was facing the wrong way.
                        //Instead of ignoring it entirely, correct the direction to as close as it can get by removing any component parallel to the triangle normal.
                        FixedV3 previousNormal = candidateNormal;
                        FixedV3.Dot(ref candidateNormal, ref triangleNormal, out dot);

                        FixedV3 p;
                        FixedV3.Multiply(ref candidateNormal, dot, out p);
                        FixedV3.Subtract(ref candidateNormal, ref p, out candidateNormal);
                        Fixed64 length = candidateNormal.LengthSquared();
                        if (length > Toolbox.Epsilon)
                        {
                            //Renormalize the corrected normal.
                            FixedV3.Divide(ref candidateNormal, Fixed64.Sqrt(length), out candidateNormal);
                            FixedV3.Dot(ref candidateNormal, ref previousNormal, out dot);
                            candidateDepth *= dot;
                        }
                        else
                        {
                            contact.PenetrationDepth = Fixed64.MaxValue;
                            contact.Normal = new FixedV3();
                        }
                    }
                    if (candidateDepth < contact.PenetrationDepth)
                    {
                        contact.Normal = candidateNormal;
                        contact.PenetrationDepth = candidateDepth;
                    }



                    //Try the depth along the positive triangle normal.

                    //If it's clockwise, this direction is unnecessary (the resulting normal would be invalidated by the onesidedness of the triangle).
                    if (triangle.sidedness != TriangleSidedness.Clockwise)
                    {
                        MPRToolbox.LocalSurfaceCast(convex, triangle, ref Toolbox.RigidIdentity, ref triangleNormal, out candidateDepth, out candidateNormal);
                        if (candidateDepth < contact.PenetrationDepth)
                        {
                            contact.Normal = candidateNormal;
                            contact.PenetrationDepth = candidateDepth;
                        }
                    }

                    //Try the depth along the negative triangle normal.

                    //If it's counterclockwise, this direction is unnecessary (the resulting normal would be invalidated by the onesidedness of the triangle).
                    if (triangle.sidedness != TriangleSidedness.Counterclockwise)
                    {
                        FixedV3.Negate(ref triangleNormal, out triangleNormal);
                        MPRToolbox.LocalSurfaceCast(convex, triangle, ref Toolbox.RigidIdentity, ref triangleNormal, out candidateDepth, out candidateNormal);
                        if (candidateDepth < contact.PenetrationDepth)
                        {
                            contact.Normal = candidateNormal;
                            contact.PenetrationDepth = candidateDepth;
                        }
                    }




                }



                MPRToolbox.RefinePenetration(convex, triangle, ref Toolbox.RigidIdentity, contact.PenetrationDepth, ref contact.Normal, out contact.PenetrationDepth, out contact.Normal, out contact.Position);

                //It's possible for the normal to still face the 'wrong' direction according to one sided triangles.
                if (triangle.sidedness != TriangleSidedness.DoubleSided)
                {
                    FixedV3.Dot(ref triangleNormal, ref contact.Normal, out dot);
                    if (dot < F64.C0)
                    {
                        //Skip the add process.
                        goto InnerSphere;
                    }
                }


                contact.Id = -1;

                if (contact.PenetrationDepth < convex.collisionMargin + triangle.collisionMargin)
                {
                    state = CollisionState.ExternalNear; //If it's emerged from the deep contact, we can go back to using the preferred GJK method.
                }
                contactList.Add(ref contact);
            }

        InnerSphere:

            if (TryInnerSphereContact(triangle, out contact))
            {
                contactList.Add(ref contact);
            }
            if (contactList.Count > 0)
                return true;

            state = CollisionState.ExternalSeparated;
            return false;












        }


        void TryToEscape()
        {
            if (++escapeAttempts == EscapeAttemptPeriod)
            {
                escapeAttempts = 0;
                state = CollisionState.Plane;
            }
        }

        void TryToEscape(TriangleShape triangle, ref FixedV3 position)
        {
            if (++escapeAttempts == EscapeAttemptPeriod && GetVoronoiRegion(triangle, ref position) == VoronoiRegion.ABC)
            {
                escapeAttempts = 0;
                state = CollisionState.Plane;
            }
        }


        private bool TryInnerSphereContact(TriangleShape triangle, out ContactData contact)
        {
            FixedV3 closestPoint;
            Toolbox.GetClosestPointOnTriangleToPoint(ref triangle.vA, ref triangle.vB, ref triangle.vC, ref Toolbox.ZeroVector, out closestPoint);
            Fixed64 length = closestPoint.LengthSquared();
            Fixed64 minimumRadius = convex.MinimumRadius * (MotionSettings.CoreShapeScaling + F64.C0p01);
            if (length < minimumRadius * minimumRadius)
            {
                FixedV3 triangleNormal, ab, ac;
                FixedV3.Subtract(ref triangle.vB, ref triangle.vA, out ab);
                FixedV3.Subtract(ref triangle.vC, ref triangle.vA, out ac);
                FixedV3.Cross(ref ab, ref ac, out triangleNormal);
                Fixed64 dot;
                FixedV3.Dot(ref closestPoint, ref triangleNormal, out dot);
                if ((triangle.sidedness == TriangleSidedness.Clockwise && dot > F64.C0) || (triangle.sidedness == TriangleSidedness.Counterclockwise && dot < F64.C0))
                {
                    //Normal was facing the wrong way.
                    contact = new ContactData();
                    return false;
                }

                length = Fixed64.Sqrt(length);
                contact.Position = closestPoint;

                if (length > Toolbox.Epsilon) //Watch out for NaN's!
                {
                    FixedV3.Divide(ref closestPoint, length, out contact.Normal);
                }
                else
                {
                    //The direction is undefined.  Use the triangle's normal.
                    //One sided triangles can only face in the appropriate direction.
                    Fixed64 normalLength = triangleNormal.LengthSquared();
                    if (triangleNormal.LengthSquared() > Toolbox.Epsilon)
                    {
                        FixedV3.Divide(ref triangleNormal, Fixed64.Sqrt(normalLength), out triangleNormal);
                        if (triangle.sidedness == TriangleSidedness.Clockwise)
                            contact.Normal = triangleNormal;
                        else
                            FixedV3.Negate(ref triangleNormal, out contact.Normal);
                    }
                    else
                    {
                        //Degenerate triangle!
                        contact = new ContactData();
                        return false;
                    }
                }

                //Compute the actual depth of the contact.
                //This is conservative; the minimum radius is guaranteed to be no larger than the shape itself.
                //But that's ok- this is strictly a deep contact protection scheme. Other contacts will make the objects separate.
                contact.PenetrationDepth = convex.MinimumRadius - length; 
                contact.Id = -1;
                return true;
            }
            contact = new ContactData();
            return false;
        }

        ///<summary>
        /// Determines what voronoi region a given point is in.
        ///</summary>
        ///<param name="p">Point to test.</param>
        ///<returns>Voronoi region containing the point.</returns>
        private VoronoiRegion GetVoronoiRegion(TriangleShape triangle, ref FixedV3 p)
        {
            //The point we are comparing against the triangle is 0,0,0, so instead of storing an "A->P" vector,
            //just use -A.
            //Same for B->, C->P...

            FixedV3 ab, ac, ap;
            FixedV3.Subtract(ref triangle.vB, ref triangle.vA, out ab);
            FixedV3.Subtract(ref triangle.vC, ref triangle.vA, out ac);
            FixedV3.Subtract(ref p, ref triangle.vA, out ap);

            //Check to see if it's outside A.
            Fixed64 APdotAB, APdotAC;
            FixedV3.Dot(ref ap, ref ab, out APdotAB);
            FixedV3.Dot(ref ap, ref ac, out APdotAC);
            if (APdotAC <= F64.C0 && APdotAB <= F64.C0)
            {
                //It is A!
                return VoronoiRegion.A;
            }

            //Check to see if it's outside B.
            Fixed64 BPdotAB, BPdotAC;
            FixedV3 bp;
            FixedV3.Subtract(ref p, ref triangle.vB, out bp);
            FixedV3.Dot(ref ab, ref bp, out BPdotAB);
            FixedV3.Dot(ref ac, ref bp, out BPdotAC);
            if (BPdotAB >= F64.C0 && BPdotAC <= BPdotAB)
            {
                //It is B!
                return VoronoiRegion.B;
            }

            //Check to see if it's outside AB.
            Fixed64 vc = APdotAB * BPdotAC - BPdotAB * APdotAC;
            if (vc <= F64.C0 && APdotAB > F64.C0 && BPdotAB < F64.C0) //Note > and < instead of => <=; avoids possibly division by zero
            {
                return VoronoiRegion.AB;
            }

            //Check to see if it's outside C.
            Fixed64 CPdotAB, CPdotAC;
            FixedV3 cp;
            FixedV3.Subtract(ref p, ref triangle.vC, out cp);
            FixedV3.Dot(ref ab, ref cp, out CPdotAB);
            FixedV3.Dot(ref ac, ref cp, out CPdotAC);
            if (CPdotAC >= F64.C0 && CPdotAB <= CPdotAC)
            {
                //It is C!
                return VoronoiRegion.C;
            }

            //Check if it's outside AC.    
            Fixed64 vb = CPdotAB * APdotAC - APdotAB * CPdotAC;
            if (vb <= F64.C0 && APdotAC > F64.C0 && CPdotAC < F64.C0) //Note > instead of >= and < instead of <=; prevents bad denominator
            {
                return VoronoiRegion.AC;
            }

            //Check if it's outside BC.
            Fixed64 va = BPdotAB * CPdotAC - CPdotAB * BPdotAC;
            if (va <= F64.C0 && (BPdotAC - BPdotAB) > F64.C0 && (CPdotAB - CPdotAC) > F64.C0)//Note > instead of >= and < instead of <=; prevents bad denominator
            {
                return VoronoiRegion.BC;
            }


            //On the face of the triangle.
            return VoronoiRegion.ABC;


        }

        ///<summary>
        /// Initializes the pair tester.
        ///</summary>
        ///<param name="convex">Convex shape to use.</param>
        public override void Initialize(ConvexShape convex)
        {
            this.convex = convex;
        }

        /// <summary>
        /// Cleans up the pair tester.
        /// </summary>
        public override void CleanUp()
        {
            convex = null;
            state = CollisionState.Plane;
            escapeAttempts = 0;
            localSeparatingAxis = new FixedV3();
            Updated = false;
        }

        internal enum CollisionState
        {
            Plane,
            ExternalSeparated,
            ExternalNear,
            Deep
        }


        public override VoronoiRegion GetRegion(TriangleShape triangle, ref ContactData contact)
        {
            //Deep contact can produce non-triangle normals while still being within the triangle.
            //To solve this problem, find the voronoi region to which the contact belongs using its normal.
            //The voronoi region will be either the most extreme vertex, or the edge that includes
            //the first and second most extreme vertices.
            //If the normal dotted with an extreme edge direction is near 0, then it belongs to the edge.
            //Otherwise, it belongs to the vertex.
            //MPR tends to produce 'approximate' normals, though.
            //Use a fairly forgiving epsilon.
            Fixed64 dotA, dotB, dotC;
            FixedV3.Dot(ref triangle.vA, ref contact.Normal, out dotA);
            FixedV3.Dot(ref triangle.vB, ref contact.Normal, out dotB);
            FixedV3.Dot(ref triangle.vC, ref contact.Normal, out dotC);

            //Since normal points from convex to triangle always, reverse dot signs.
            dotA = -dotA;
            dotB = -dotB;
            dotC = -dotC;


            Fixed64 faceEpsilon = F64.C0p01;
            Fixed64 edgeEpsilon = F64.C0p01;

            Fixed64 edgeDot;
            FixedV3 edgeDirection;
            if (dotA > dotB && dotA > dotC)
            {
                //A is extreme.
                if (dotB > dotC)
                {
                    //B is second most extreme.
                    if (Fixed64.Abs(dotA - dotC) < faceEpsilon)
                    {
                        //The normal is basically a face normal.  This can happen at the edges occasionally.
                        return VoronoiRegion.ABC;
                    }
                    else
                    {
                        FixedV3.Subtract(ref triangle.vB, ref triangle.vA, out edgeDirection);
                        FixedV3.Dot(ref edgeDirection, ref contact.Normal, out edgeDot);
                        if (edgeDot * edgeDot < edgeDirection.LengthSquared() * edgeEpsilon)
                            return VoronoiRegion.AB;
                        else
                            return VoronoiRegion.A;
                    }
                }
                else
                {
                    //C is second most extreme.
                    if (Fixed64.Abs(dotA - dotB) < faceEpsilon)
                    {
                        //The normal is basically a face normal.  This can happen at the edges occasionally.
                        return VoronoiRegion.ABC;
                    }
                    else
                    {
                        FixedV3.Subtract(ref triangle.vC, ref triangle.vA, out edgeDirection);
                        FixedV3.Dot(ref edgeDirection, ref contact.Normal, out edgeDot);
                        if (edgeDot * edgeDot < edgeDirection.LengthSquared() * edgeEpsilon)
                            return VoronoiRegion.AC;
                        else
                            return VoronoiRegion.A;
                    }
                }
            }
            else if (dotB > dotC)
            {
                //B is extreme.
                if (dotC > dotA)
                {
                    //C is second most extreme.
                    if (Fixed64.Abs(dotB - dotA) < faceEpsilon)
                    {
                        //The normal is basically a face normal.  This can happen at the edges occasionally.
                        return VoronoiRegion.ABC;
                    }
                    else
                    {
                        FixedV3.Subtract(ref triangle.vC, ref triangle.vB, out edgeDirection);
                        FixedV3.Dot(ref edgeDirection, ref contact.Normal, out edgeDot);
                        if (edgeDot * edgeDot < edgeDirection.LengthSquared() * edgeEpsilon)
                            return VoronoiRegion.BC;
                        else
                            return VoronoiRegion.B;
                    }
                }
                else
                {
                    //A is second most extreme.
                    if (Fixed64.Abs(dotB - dotC) < faceEpsilon)
                    {
                        //The normal is basically a face normal.  This can happen at the edges occasionally.
                        return VoronoiRegion.ABC;
                    }
                    else
                    {
                        FixedV3.Subtract(ref triangle.vA, ref triangle.vB, out edgeDirection);
                        FixedV3.Dot(ref edgeDirection, ref contact.Normal, out edgeDot);
                        if (edgeDot * edgeDot < edgeDirection.LengthSquared() * edgeEpsilon)
                            return VoronoiRegion.AB;
                        else
                            return VoronoiRegion.B;
                    }
                }
            }
            else
            {
                //C is extreme.
                if (dotA > dotB)
                {
                    //A is second most extreme.
                    if (Fixed64.Abs(dotC - dotB) < faceEpsilon)
                    {
                        //The normal is basically a face normal.  This can happen at the edges occasionally.
                        return VoronoiRegion.ABC;
                    }
                    else
                    {
                        FixedV3.Subtract(ref triangle.vA, ref triangle.vC, out edgeDirection);
                        FixedV3.Dot(ref edgeDirection, ref contact.Normal, out edgeDot);
                        if (edgeDot * edgeDot < edgeDirection.LengthSquared() * edgeEpsilon)
                            return VoronoiRegion.AC;
                        else
                            return VoronoiRegion.C;
                    }
                }
                else
                {
                    //B is second most extreme.
                    if (Fixed64.Abs(dotC - dotA) < faceEpsilon)
                    {
                        //The normal is basically a face normal.  This can happen at the edges occasionally.
                        return VoronoiRegion.ABC;
                    }
                    else
                    {
                        FixedV3.Subtract(ref triangle.vB, ref triangle.vC, out edgeDirection);
                        FixedV3.Dot(ref edgeDirection, ref contact.Normal, out edgeDot);
                        if (edgeDot * edgeDot < edgeDirection.LengthSquared() * edgeEpsilon)
                            return VoronoiRegion.BC;
                        else
                            return VoronoiRegion.C;
                    }
                }
            }

        }

        public override bool ShouldCorrectContactNormal
        {
            get
            {
                return state == CollisionState.Deep;
            }
        }

    }

}

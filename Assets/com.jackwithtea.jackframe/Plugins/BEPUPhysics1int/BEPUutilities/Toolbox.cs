using System;
using System.Collections.Generic;
using BEPUPhysics1int.CollisionTests;
using BEPUPhysics1int.DataStructures;
using BEPUPhysics1int.ResourceManagement;
using FixMath.NET;

namespace BEPUPhysics1int
{
    //TODO: It would be nice to split and improve this monolith into individually superior, organized components.


    /// <summary>
    /// Helper class with many algorithms for intersection testing and 3D math.
    /// </summary>
    public static class Toolbox
    {
        /// <summary>
        /// Large tolerance value. Defaults to 1e-5f.
        /// </summary>
        public static Fixed64 BigEpsilon = F64.C1 / new Fixed64(100000);

        /// <summary>
        /// Tolerance value. Defaults to 1e-7f.
        /// </summary>
        public static Fixed64 Epsilon = F64.C1 / new Fixed64(10000000);

        /// <summary>
        /// Represents an invalid Vector3.
        /// </summary>
        public static readonly FixedV3 NoVector = new FixedV3(-Fixed64.MaxValue, -Fixed64.MaxValue, -Fixed64.MaxValue);

        /// <summary>
        /// Reference for a vector with dimensions (0,0,1).
        /// </summary>
        public static FixedV3 BackVector = FixedV3.Backward;

        /// <summary>
        /// Reference for a vector with dimensions (0,-1,0).
        /// </summary>
        public static FixedV3 DownVector = FixedV3.Down;

        /// <summary>
        /// Reference for a vector with dimensions (0,0,-1).
        /// </summary>
        public static FixedV3 ForwardVector = FixedV3.Forward;

        /// <summary>
        /// Refers to the identity quaternion.
        /// </summary>
        public static FixedQuaternion IdentityOrientation = FixedQuaternion.Identity;

        /// <summary>
        /// Reference for a vector with dimensions (-1,0,0).
        /// </summary>
        public static FixedV3 LeftVector = FixedV3.Left;

        /// <summary>
        /// Reference for a vector with dimensions (1,0,0).
        /// </summary>
        public static FixedV3 RightVector = FixedV3.Right;

        /// <summary>
        /// Reference for a vector with dimensions (0,1,0).
        /// </summary>
        public static FixedV3 UpVector = FixedV3.Up;

        /// <summary>
        /// Matrix containing zeroes for every element.
        /// </summary>
        public static BEPUMatrix ZeroMatrix = new BEPUMatrix(F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0);

        /// <summary>
        /// Reference for a vector with dimensions (0,0,0).
        /// </summary>
        public static FixedV3 ZeroVector = FixedV3.Zero;

        /// <summary>
        /// Refers to the rigid identity transformation.
        /// </summary>
        public static RigidTransform RigidIdentity = RigidTransform.Identity;

        #region Segment/Ray-Triangle Tests

        /// <summary>
        /// Determines the intersection between a ray and a triangle.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length to travel in units of the direction's length.</param>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="hitClockwise">True if the the triangle was hit on the clockwise face, false otherwise.</param>
        /// <param name="hit">Hit data of the ray, if any</param>
        /// <returns>Whether or not the ray and triangle intersect.</returns>
        public static bool FindRayTriangleIntersection(ref BEPURay ray, Fixed64 maximumLength, ref FixedV3 a, ref FixedV3 b, ref FixedV3 c, out bool hitClockwise, out RayHit hit)
        {
            hitClockwise = false;
            hit = new RayHit();
            FixedV3 ab, ac;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3.Subtract(ref c, ref a, out ac);

            FixedV3.Cross(ref ab, ref ac, out hit.Normal);
            if (hit.Normal.LengthSquared() < Epsilon)
                return false; //Degenerate triangle!

            Fixed64 d;
            FixedV3.Dot(ref ray.Direction, ref hit.Normal, out d);
            d = -d;

            hitClockwise = d >= F64.C0;

            FixedV3 ap;
            FixedV3.Subtract(ref ray.Position, ref a, out ap);

            FixedV3.Dot(ref ap, ref hit.Normal, out hit.T);
            hit.T /= d;
            if (hit.T < F64.C0 || hit.T > maximumLength)
                return false;//Hit is behind origin, or too far away.

            FixedV3.Multiply(ref ray.Direction, hit.T, out hit.Location);
            FixedV3.Add(ref ray.Position, ref hit.Location, out hit.Location);

            // Compute barycentric coordinates
            FixedV3.Subtract(ref hit.Location, ref a, out ap);
            Fixed64 ABdotAB, ABdotAC, ABdotAP;
            Fixed64 ACdotAC, ACdotAP;
            FixedV3.Dot(ref ab, ref ab, out ABdotAB);
            FixedV3.Dot(ref ab, ref ac, out ABdotAC);
            FixedV3.Dot(ref ab, ref ap, out ABdotAP);
            FixedV3.Dot(ref ac, ref ac, out ACdotAC);
            FixedV3.Dot(ref ac, ref ap, out ACdotAP);

            Fixed64 denom = F64.C1 / (ABdotAB * ACdotAC - ABdotAC * ABdotAC);
            Fixed64 u = (ACdotAC * ABdotAP - ABdotAC * ACdotAP) * denom;
            Fixed64 v = (ABdotAB * ACdotAP - ABdotAC * ABdotAP) * denom;

            return (u >= -Toolbox.BigEpsilon) && (v >= -Toolbox.BigEpsilon) && (u + v <= F64.C1 + Toolbox.BigEpsilon);

        }

        /// <summary>
        /// Determines the intersection between a ray and a triangle.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length to travel in units of the direction's length.</param>
        /// <param name="sidedness">Sidedness of the triangle to test.</param>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="hit">Hit data of the ray, if any</param>
        /// <returns>Whether or not the ray and triangle intersect.</returns>
        public static bool FindRayTriangleIntersection(ref BEPURay ray, Fixed64 maximumLength, TriangleSidedness sidedness, ref FixedV3 a, ref FixedV3 b, ref FixedV3 c, out RayHit hit)
        {
            hit = new RayHit();
            FixedV3 ab, ac;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3.Subtract(ref c, ref a, out ac);

            FixedV3.Cross(ref ab, ref ac, out hit.Normal);
            if (hit.Normal.LengthSquared() < Epsilon)
                return false; //Degenerate triangle!

            Fixed64 d;
            FixedV3.Dot(ref ray.Direction, ref hit.Normal, out d);
            d = -d;
            switch (sidedness)
            {
                case TriangleSidedness.DoubleSided:
                    if (d <= F64.C0) //Pointing the wrong way.  Flip the normal.
                    {
                        FixedV3.Negate(ref hit.Normal, out hit.Normal);
                        d = -d;
                    }
                    break;
                case TriangleSidedness.Clockwise:
                    if (d <= F64.C0) //Pointing the wrong way.  Can't hit.
                        return false;

                    break;
                case TriangleSidedness.Counterclockwise:
                    if (d >= F64.C0) //Pointing the wrong way.  Can't hit.
                        return false;

                    FixedV3.Negate(ref hit.Normal, out hit.Normal);
                    d = -d;
                    break;
            }

            FixedV3 ap;
            FixedV3.Subtract(ref ray.Position, ref a, out ap);

            FixedV3.Dot(ref ap, ref hit.Normal, out hit.T);
            hit.T /= d;
            if (hit.T < F64.C0 || hit.T > maximumLength)
                return false;//Hit is behind origin, or too far away.

            FixedV3.Multiply(ref ray.Direction, hit.T, out hit.Location);
            FixedV3.Add(ref ray.Position, ref hit.Location, out hit.Location);

            // Compute barycentric coordinates
            FixedV3.Subtract(ref hit.Location, ref a, out ap);
            Fixed64 ABdotAB, ABdotAC, ABdotAP;
            Fixed64 ACdotAC, ACdotAP;
            FixedV3.Dot(ref ab, ref ab, out ABdotAB);
            FixedV3.Dot(ref ab, ref ac, out ABdotAC);
            FixedV3.Dot(ref ab, ref ap, out ABdotAP);
            FixedV3.Dot(ref ac, ref ac, out ACdotAC);
            FixedV3.Dot(ref ac, ref ap, out ACdotAP);

            Fixed64 denom = F64.C1 / (ABdotAB * ACdotAC - ABdotAC * ABdotAC);
            Fixed64 u = (ACdotAC * ABdotAP - ABdotAC * ACdotAP) * denom;
            Fixed64 v = (ABdotAB * ACdotAP - ABdotAC * ABdotAP) * denom;

            return (u >= -Toolbox.BigEpsilon) && (v >= -Toolbox.BigEpsilon) && (u + v <= F64.C1 + Toolbox.BigEpsilon);

        }

        /// <summary>
        /// Finds the intersection between the given segment and the given plane defined by three points.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="d">First vertex of a triangle which lies on the plane.</param>
        /// <param name="e">Second vertex of a triangle which lies on the plane.</param>
        /// <param name="f">Third vertex of a triangle which lies on the plane.</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the segment intersects the plane.</returns>
        public static bool GetSegmentPlaneIntersection(FixedV3 a, FixedV3 b, FixedV3 d, FixedV3 e, FixedV3 f, out FixedV3 q)
        {
            BEPUPlane p;
            p.Normal = FixedV3.Cross(e - d, f - d);
            p.D = FixedV3.Dot(p.Normal, d);
            Fixed64 t;
            return GetSegmentPlaneIntersection(a, b, p, out t, out q);
        }

        /// <summary>
        /// Finds the intersection between the given segment and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second enpoint of segment.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the segment intersects the plane.</returns>
        public static bool GetSegmentPlaneIntersection(FixedV3 a, FixedV3 b, BEPUPlane p, out FixedV3 q)
        {
            Fixed64 t;
            return GetLinePlaneIntersection(ref a, ref b, ref p, out t, out q) && t >= F64.C0 && t <= F64.C1;
        }

        /// <summary>
        /// Finds the intersection between the given segment and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along segment to intersection.</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the segment intersects the plane.</returns>
        public static bool GetSegmentPlaneIntersection(FixedV3 a, FixedV3 b, BEPUPlane p, out Fixed64 t, out FixedV3 q)
        {
            return GetLinePlaneIntersection(ref a, ref b, ref p, out t, out q) && t >= F64.C0 && t <= F64.C1;
        }

        /// <summary>
        /// Finds the intersection between the given line and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment defining the line.</param>
        /// <param name="b">Second endpoint of segment defining the line.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along line to intersection (A + t * AB).</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the line intersects the plane.  If false, the line is parallel to the plane's surface.</returns>
        public static bool GetLinePlaneIntersection(ref FixedV3 a, ref FixedV3 b, ref BEPUPlane p, out Fixed64 t, out FixedV3 q)
        {
            FixedV3 ab;
            FixedV3.Subtract(ref b, ref a, out ab);
            Fixed64 denominator;
            FixedV3.Dot(ref p.Normal, ref ab, out denominator);
            if (denominator < Epsilon && denominator > -Epsilon)
            {
                //Surface of plane and line are parallel (or very close to it).
                q = new FixedV3();
                t = Fixed64.MaxValue;
                return false;
            }
            Fixed64 numerator;
            FixedV3.Dot(ref p.Normal, ref a, out numerator);
            t = (p.D - numerator) / denominator;
            //Compute the intersection position.
            FixedV3.Multiply(ref ab, t, out q);
            FixedV3.Add(ref a, ref q, out q);
            return true;
        }

        /// <summary>
        /// Finds the intersection between the given ray and the given plane.
        /// </summary>
        /// <param name="ray">Ray to test against the plane.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along line to intersection (A + t * AB).</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the line intersects the plane.  If false, the line is parallel to the plane's surface.</returns>
        public static bool GetRayPlaneIntersection(ref BEPURay ray, ref BEPUPlane p, out Fixed64 t, out FixedV3 q)
        {
            Fixed64 denominator;
            FixedV3.Dot(ref p.Normal, ref ray.Direction, out denominator);
            if (denominator < Epsilon && denominator > -Epsilon)
            {
                //Surface of plane and line are parallel (or very close to it).
                q = new FixedV3();
                t = Fixed64.MaxValue;
                return false;
            }
            Fixed64 numerator;
            FixedV3.Dot(ref p.Normal, ref ray.Position, out numerator);
            t = (p.D - numerator) / denominator;
            //Compute the intersection position.
            FixedV3.Multiply(ref ray.Direction, t, out q);
            FixedV3.Add(ref ray.Position, ref q, out q);
            return t >= F64.C0;
        }

        #endregion

        #region Point-Triangle Tests

        /// <summary>
        /// Determines the closest point on a triangle given by points a, b, and c to point p.
        /// </summary>
        /// <param name="a">First vertex of triangle.</param>
        /// <param name="b">Second vertex of triangle.</param>
        /// <param name="c">Third vertex of triangle.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="closestPoint">Closest point on tetrahedron to point.</param>
        /// <returns>Voronoi region containing the closest point.</returns>
        public static VoronoiRegion GetClosestPointOnTriangleToPoint(ref FixedV3 a, ref FixedV3 b, ref FixedV3 c, ref FixedV3 p, out FixedV3 closestPoint)
        {
            Fixed64 v, w;
            FixedV3 ab;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3 ac;
            FixedV3.Subtract(ref c, ref a, out ac);
            //Vertex region A?
            FixedV3 ap;
            FixedV3.Subtract(ref p, ref a, out ap);
            Fixed64 d1;
            FixedV3.Dot(ref ab, ref ap, out d1);
            Fixed64 d2;
            FixedV3.Dot(ref ac, ref ap, out d2);
            if (d1 <= F64.C0 && d2 < F64.C0)
            {
                closestPoint = a;
                return VoronoiRegion.A;
            }
            //Vertex region B?
            FixedV3 bp;
            FixedV3.Subtract(ref p, ref b, out bp);
            Fixed64 d3;
            FixedV3.Dot(ref ab, ref bp, out d3);
            Fixed64 d4;
            FixedV3.Dot(ref ac, ref bp, out d4);
            if (d3 >= F64.C0 && d4 <= d3)
            {
                closestPoint = b;
                return VoronoiRegion.B;
            }
            //Edge region AB?
            Fixed64 vc = d1 * d4 - d3 * d2;
            if (vc <= F64.C0 && d1 >= F64.C0 && d3 <= F64.C0)
            {
                v = d1 / (d1 - d3);
                FixedV3.Multiply(ref ab, v, out closestPoint);
                FixedV3.Add(ref closestPoint, ref a, out closestPoint);
                return VoronoiRegion.AB;
            }
            //Vertex region C?
            FixedV3 cp;
            FixedV3.Subtract(ref p, ref c, out cp);
            Fixed64 d5;
            FixedV3.Dot(ref ab, ref cp, out d5);
            Fixed64 d6;
            FixedV3.Dot(ref ac, ref cp, out d6);
            if (d6 >= F64.C0 && d5 <= d6)
            {
                closestPoint = c;
                return VoronoiRegion.C;
            }
            //Edge region AC?
            Fixed64 vb = d5 * d2 - d1 * d6;
            if (vb <= F64.C0 && d2 >= F64.C0 && d6 <= F64.C0)
            {
                w = d2 / (d2 - d6);
                FixedV3.Multiply(ref ac, w, out closestPoint);
                FixedV3.Add(ref closestPoint, ref a, out closestPoint);
                return VoronoiRegion.AC;
            }
            //Edge region BC?
            Fixed64 va = d3 * d6 - d5 * d4;
            if (va <= F64.C0 && (d4 - d3) >= F64.C0 && (d5 - d6) >= F64.C0)
            {
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                FixedV3.Subtract(ref c, ref b, out closestPoint);
                FixedV3.Multiply(ref closestPoint, w, out closestPoint);
                FixedV3.Add(ref closestPoint, ref b, out closestPoint);
                return VoronoiRegion.BC;
            }
            //Inside triangle?
            Fixed64 denom = F64.C1 / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;
            FixedV3 abv;
            FixedV3.Multiply(ref ab, v, out abv);
            FixedV3 acw;
            FixedV3.Multiply(ref ac, w, out acw);
            FixedV3.Add(ref a, ref abv, out closestPoint);
            FixedV3.Add(ref closestPoint, ref acw, out closestPoint);
            return VoronoiRegion.ABC;
        }

        /// <summary>
        /// Determines the closest point on a triangle given by points a, b, and c to point p and provides the subsimplex whose voronoi region contains the point.
        /// </summary>
        /// <param name="a">First vertex of triangle.</param>
        /// <param name="b">Second vertex of triangle.</param>
        /// <param name="c">Third vertex of triangle.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point.</param>
        /// <param name="closestPoint">Closest point on tetrahedron to point.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnTriangleToPoint(ref FixedV3 a, ref FixedV3 b, ref FixedV3 c, ref FixedV3 p, RawList<FixedV3> subsimplex, out FixedV3 closestPoint)
        {
            subsimplex.Clear();
            Fixed64 v, w;
            FixedV3 ab;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3 ac;
            FixedV3.Subtract(ref c, ref a, out ac);
            //Vertex region A?
            FixedV3 ap;
            FixedV3.Subtract(ref p, ref a, out ap);
            Fixed64 d1;
            FixedV3.Dot(ref ab, ref ap, out d1);
            Fixed64 d2;
            FixedV3.Dot(ref ac, ref ap, out d2);
            if (d1 <= F64.C0 && d2 < F64.C0)
            {
                subsimplex.Add(a);
                closestPoint = a;
                return;
            }
            //Vertex region B?
            FixedV3 bp;
            FixedV3.Subtract(ref p, ref b, out bp);
            Fixed64 d3;
            FixedV3.Dot(ref ab, ref bp, out d3);
            Fixed64 d4;
            FixedV3.Dot(ref ac, ref bp, out d4);
            if (d3 >= F64.C0 && d4 <= d3)
            {
                subsimplex.Add(b);
                closestPoint = b;
                return;
            }
            //Edge region AB?
            Fixed64 vc = d1 * d4 - d3 * d2;
            if (vc <= F64.C0 && d1 >= F64.C0 && d3 <= F64.C0)
            {
                subsimplex.Add(a);
                subsimplex.Add(b);
                v = d1 / (d1 - d3);
                FixedV3.Multiply(ref ab, v, out closestPoint);
                FixedV3.Add(ref closestPoint, ref a, out closestPoint);
                return;
            }
            //Vertex region C?
            FixedV3 cp;
            FixedV3.Subtract(ref p, ref c, out cp);
            Fixed64 d5;
            FixedV3.Dot(ref ab, ref cp, out d5);
            Fixed64 d6;
            FixedV3.Dot(ref ac, ref cp, out d6);
            if (d6 >= F64.C0 && d5 <= d6)
            {
                subsimplex.Add(c);
                closestPoint = c;
                return;
            }
            //Edge region AC?
            Fixed64 vb = d5 * d2 - d1 * d6;
            if (vb <= F64.C0 && d2 >= F64.C0 && d6 <= F64.C0)
            {
                subsimplex.Add(a);
                subsimplex.Add(c);
                w = d2 / (d2 - d6);
                FixedV3.Multiply(ref ac, w, out closestPoint);
                FixedV3.Add(ref closestPoint, ref a, out closestPoint);
                return;
            }
            //Edge region BC?
            Fixed64 va = d3 * d6 - d5 * d4;
            if (va <= F64.C0 && (d4 - d3) >= F64.C0 && (d5 - d6) >= F64.C0)
            {
                subsimplex.Add(b);
                subsimplex.Add(c);
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                FixedV3.Subtract(ref c, ref b, out closestPoint);
                FixedV3.Multiply(ref closestPoint, w, out closestPoint);
                FixedV3.Add(ref closestPoint, ref b, out closestPoint);
                return;
            }
            //Inside triangle?
            subsimplex.Add(a);
            subsimplex.Add(b);
            subsimplex.Add(c);
            Fixed64 denom = F64.C1 / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;
            FixedV3 abv;
            FixedV3.Multiply(ref ab, v, out abv);
            FixedV3 acw;
            FixedV3.Multiply(ref ac, w, out acw);
            FixedV3.Add(ref a, ref abv, out closestPoint);
            FixedV3.Add(ref closestPoint, ref acw, out closestPoint);
        }

        /// <summary>
        /// Determines the closest point on a triangle given by points a, b, and c to point p and provides the subsimplex whose voronoi region contains the point.
        /// </summary>
        /// <param name="q">Simplex containing triangle for testing.</param>
        /// <param name="i">Index of first vertex of triangle.</param>
        /// <param name="j">Index of second vertex of triangle.</param>
        /// <param name="k">Index of third vertex of triangle.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point, enumerated as a = 0, b = 1, c = 2.</param>
        /// <param name="baryCoords">Barycentric coordinates of the point on the triangle.</param>
        /// <param name="closestPoint">Closest point on tetrahedron to point.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnTriangleToPoint(RawList<FixedV3> q, int i, int j, int k, ref FixedV3 p, RawList<int> subsimplex, RawList<Fixed64> baryCoords, out FixedV3 closestPoint)
        {
            subsimplex.Clear();
            baryCoords.Clear();
            Fixed64 v, w;
            FixedV3 a = q[i];
            FixedV3 b = q[j];
            FixedV3 c = q[k];
            FixedV3 ab;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3 ac;
            FixedV3.Subtract(ref c, ref a, out ac);
            //Vertex region A?
            FixedV3 ap;
            FixedV3.Subtract(ref p, ref a, out ap);
            Fixed64 d1;
            FixedV3.Dot(ref ab, ref ap, out d1);
            Fixed64 d2;
            FixedV3.Dot(ref ac, ref ap, out d2);
            if (d1 <= F64.C0 && d2 < F64.C0)
            {
                subsimplex.Add(i);
                baryCoords.Add(F64.C1);
                closestPoint = a;
                return; //barycentric coordinates (1,0,0)
            }
            //Vertex region B?
            FixedV3 bp;
            FixedV3.Subtract(ref p, ref b, out bp);
            Fixed64 d3;
            FixedV3.Dot(ref ab, ref bp, out d3);
            Fixed64 d4;
            FixedV3.Dot(ref ac, ref bp, out d4);
            if (d3 >= F64.C0 && d4 <= d3)
            {
                subsimplex.Add(j);
                baryCoords.Add(F64.C1);
                closestPoint = b;
                return; //barycentric coordinates (0,1,0)
            }
            //Edge region AB?
            Fixed64 vc = d1 * d4 - d3 * d2;
            if (vc <= F64.C0 && d1 >= F64.C0 && d3 <= F64.C0)
            {
                subsimplex.Add(i);
                subsimplex.Add(j);
                v = d1 / (d1 - d3);
                baryCoords.Add(F64.C1 - v);
                baryCoords.Add(v);
                FixedV3.Multiply(ref ab, v, out closestPoint);
                FixedV3.Add(ref closestPoint, ref a, out closestPoint);
                return; //barycentric coordinates (1-v, v, 0)
            }
            //Vertex region C?
            FixedV3 cp;
            FixedV3.Subtract(ref p, ref c, out cp);
            Fixed64 d5;
            FixedV3.Dot(ref ab, ref cp, out d5);
            Fixed64 d6;
            FixedV3.Dot(ref ac, ref cp, out d6);
            if (d6 >= F64.C0 && d5 <= d6)
            {
                subsimplex.Add(k);
                baryCoords.Add(F64.C1);
                closestPoint = c;
                return; //barycentric coordinates (0,0,1)
            }
            //Edge region AC?
            Fixed64 vb = d5 * d2 - d1 * d6;
            if (vb <= F64.C0 && d2 >= F64.C0 && d6 <= F64.C0)
            {
                subsimplex.Add(i);
                subsimplex.Add(k);
                w = d2 / (d2 - d6);
                baryCoords.Add(F64.C1 - w);
                baryCoords.Add(w);
                FixedV3.Multiply(ref ac, w, out closestPoint);
                FixedV3.Add(ref closestPoint, ref a, out closestPoint);
                return; //barycentric coordinates (1-w, 0, w)
            }
            //Edge region BC?
            Fixed64 va = d3 * d6 - d5 * d4;
            if (va <= F64.C0 && (d4 - d3) >= F64.C0 && (d5 - d6) >= F64.C0)
            {
                subsimplex.Add(j);
                subsimplex.Add(k);
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                baryCoords.Add(F64.C1 - w);
                baryCoords.Add(w);
                FixedV3.Subtract(ref c, ref b, out closestPoint);
                FixedV3.Multiply(ref closestPoint, w, out closestPoint);
                FixedV3.Add(ref closestPoint, ref b, out closestPoint);
                return; //barycentric coordinates (0, 1 - w ,w)
            }
            //Inside triangle?
            subsimplex.Add(i);
            subsimplex.Add(j);
            subsimplex.Add(k);
            Fixed64 denom = F64.C1 / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;
            baryCoords.Add(F64.C1 - v - w);
            baryCoords.Add(v);
            baryCoords.Add(w);
            FixedV3 abv;
            FixedV3.Multiply(ref ab, v, out abv);
            FixedV3 acw;
            FixedV3.Multiply(ref ac, w, out acw);
            FixedV3.Add(ref a, ref abv, out closestPoint);
            FixedV3.Add(ref closestPoint, ref acw, out closestPoint);
            //return a + ab * v + ac * w; //barycentric coordinates (1 - v - w, v, w)
        }

        /// <summary>
        /// Determines if supplied point is within the triangle as defined by the provided vertices.
        /// </summary>
        /// <param name="vA">A vertex of the triangle.</param>
        /// <param name="vB">A vertex of the triangle.</param>
        /// <param name="vC">A vertex of the triangle.</param>
        /// <param name="p">The point for comparison against the triangle.</param>
        /// <returns>Whether or not the point is within the triangle.</returns>
        public static bool IsPointInsideTriangle(ref FixedV3 vA, ref FixedV3 vB, ref FixedV3 vC, ref FixedV3 p)
        {
            Fixed64 u, v, w;
            GetBarycentricCoordinates(ref p, ref vA, ref vB, ref vC, out u, out v, out w);
            //Are the barycoords valid?
            return (u > -Epsilon) && (v > -Epsilon) && (w > -Epsilon);
        }

        /// <summary>
        /// Determines if supplied point is within the triangle as defined by the provided vertices.
        /// </summary>
        /// <param name="vA">A vertex of the triangle.</param>
        /// <param name="vB">A vertex of the triangle.</param>
        /// <param name="vC">A vertex of the triangle.</param>
        /// <param name="p">The point for comparison against the triangle.</param>
        /// <param name="margin">Extra area on the edges of the triangle to include.  Can be negative.</param>
        /// <returns>Whether or not the point is within the triangle.</returns>
        public static bool IsPointInsideTriangle(ref FixedV3 vA, ref FixedV3 vB, ref FixedV3 vC, ref FixedV3 p, Fixed64 margin)
        {
            Fixed64 u, v, w;
            GetBarycentricCoordinates(ref p, ref vA, ref vB, ref vC, out u, out v, out w);
            //Are the barycoords valid?
            return (u > -margin) && (v > -margin) && (w > -margin);
        }

        #endregion

        #region Point-Line Tests

        /// <summary>
        /// Determines the closest point on the provided segment ab to point p.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="closestPoint">Closest point on the edge to p.</param>
        public static void GetClosestPointOnSegmentToPoint(ref FixedV3 a, ref FixedV3 b, ref FixedV3 p, out FixedV3 closestPoint)
        {
            FixedV3 ab;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3 ap;
            FixedV3.Subtract(ref p, ref a, out ap);
            Fixed64 t;
            FixedV3.Dot(ref ap, ref ab, out t);
            if (t <= F64.C0)
            {
                closestPoint = a;
            }
            else
            {
                Fixed64 denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
                if (t >= denom)
                {
                    closestPoint = b;
                }
                else
                {
                    t = t / denom;
                    FixedV3 tab;
                    FixedV3.Multiply(ref ab, t, out tab);
                    FixedV3.Add(ref a, ref tab, out closestPoint);
                }
            }
        }

        /// <summary>
        /// Determines the closest point on the provided segment ab to point p.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point.</param>
        /// <param name="closestPoint">Closest point on the edge to p.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnSegmentToPoint(ref FixedV3 a, ref FixedV3 b, ref FixedV3 p, List<FixedV3> subsimplex, out FixedV3 closestPoint)
        {
            subsimplex.Clear();
            FixedV3 ab;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3 ap;
            FixedV3.Subtract(ref p, ref a, out ap);
            Fixed64 t;
            FixedV3.Dot(ref ap, ref ab, out t);
            if (t <= F64.C0)
            {
                //t = 0;//Don't need this for returning purposes.
                subsimplex.Add(a);
                closestPoint = a;
            }
            else
            {
                Fixed64 denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
                if (t >= denom)
                {
                    //t = 1;//Don't need this for returning purposes.
                    subsimplex.Add(b);
                    closestPoint = b;
                }
                else
                {
                    t = t / denom;
                    subsimplex.Add(a);
                    subsimplex.Add(b);
                    FixedV3 tab;
                    FixedV3.Multiply(ref ab, t, out tab);
                    FixedV3.Add(ref a, ref tab, out closestPoint);
                }
            }
        }

        /// <summary>
        /// Determines the closest point on the provided segment ab to point p.
        /// </summary>
        /// <param name="q">List of points in the containing simplex.</param>
        /// <param name="i">Index of first endpoint of segment.</param>
        /// <param name="j">Index of second endpoint of segment.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point, enumerated as a = 0, b = 1.</param>
        /// <param name="baryCoords">Barycentric coordinates of the point.</param>
        /// <param name="closestPoint">Closest point on the edge to p.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnSegmentToPoint(List<FixedV3> q, int i, int j, ref FixedV3 p, List<int> subsimplex, List<Fixed64> baryCoords, out FixedV3 closestPoint)
        {
            FixedV3 a = q[i];
            FixedV3 b = q[j];
            subsimplex.Clear();
            baryCoords.Clear();
            FixedV3 ab;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3 ap;
            FixedV3.Subtract(ref p, ref a, out ap);
            Fixed64 t;
            FixedV3.Dot(ref ap, ref ab, out t);
            if (t <= F64.C0)
            {
                subsimplex.Add(i);
                baryCoords.Add(F64.C1);
                closestPoint = a;
            }
            else
            {
                Fixed64 denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
                if (t >= denom)
                {
                    subsimplex.Add(j);
                    baryCoords.Add(F64.C1);
                    closestPoint = b;
                }
                else
                {
                    t = t / denom;
                    subsimplex.Add(i);
                    subsimplex.Add(j);
                    baryCoords.Add(F64.C1 - t);
                    baryCoords.Add(t);
                    FixedV3 tab;
                    FixedV3.Multiply(ref ab, t, out tab);
                    FixedV3.Add(ref a, ref tab, out closestPoint);
                }
            }
        }


        /// <summary>
        /// Determines the shortest squared distance from the point to the line.
        /// </summary>
        /// <param name="p">Point to check against the line.</param>
        /// <param name="a">First point on the line.</param>
        /// <param name="b">Second point on the line.</param>
        /// <returns>Shortest squared distance from the point to the line.</returns>
        public static Fixed64 GetSquaredDistanceFromPointToLine(ref FixedV3 p, ref FixedV3 a, ref FixedV3 b)
        {
            FixedV3 ap, ab;
            FixedV3.Subtract(ref p, ref a, out ap);
            FixedV3.Subtract(ref b, ref a, out ab);
            Fixed64 e;
            FixedV3.Dot(ref ap, ref ab, out e);
            return ap.LengthSquared() - e * e / ab.LengthSquared();
        }

        #endregion

        #region Line-Line Tests

        /// <summary>
        /// Computes closest points c1 and c2 betwen segments p1q1 and p2q2.
        /// </summary>
        /// <param name="p1">First point of first segment.</param>
        /// <param name="q1">Second point of first segment.</param>
        /// <param name="p2">First point of second segment.</param>
        /// <param name="q2">Second point of second segment.</param>
        /// <param name="c1">Closest point on first segment.</param>
        /// <param name="c2">Closest point on second segment.</param>
        public static void GetClosestPointsBetweenSegments(FixedV3 p1, FixedV3 q1, FixedV3 p2, FixedV3 q2, out FixedV3 c1, out FixedV3 c2)
        {
			Fixed64 s, t;
            GetClosestPointsBetweenSegments(ref p1, ref q1, ref p2, ref q2, out s, out t, out c1, out c2);
        }

        /// <summary>
        /// Computes closest points c1 and c2 betwen segments p1q1 and p2q2.
        /// </summary>
        /// <param name="p1">First point of first segment.</param>
        /// <param name="q1">Second point of first segment.</param>
        /// <param name="p2">First point of second segment.</param>
        /// <param name="q2">Second point of second segment.</param>
        /// <param name="s">Distance along the line to the point for first segment.</param>
        /// <param name="t">Distance along the line to the point for second segment.</param>
        /// <param name="c1">Closest point on first segment.</param>
        /// <param name="c2">Closest point on second segment.</param>
        public static void GetClosestPointsBetweenSegments(ref FixedV3 p1, ref FixedV3 q1, ref FixedV3 p2, ref FixedV3 q2,
                                                           out Fixed64 s, out Fixed64 t, out FixedV3 c1, out FixedV3 c2)
        {
            //Segment direction vectors
            FixedV3 d1;
            FixedV3.Subtract(ref q1, ref p1, out d1);
            FixedV3 d2;
            FixedV3.Subtract(ref q2, ref p2, out d2);
            FixedV3 r;
            FixedV3.Subtract(ref p1, ref p2, out r);
            //distance
            Fixed64 a = d1.LengthSquared();
            Fixed64 e = d2.LengthSquared();
            Fixed64 f;
            FixedV3.Dot(ref d2, ref r, out f);

            if (a <= Epsilon && e <= Epsilon)
            {
                //These segments are more like points.
                s = t = F64.C0;
                c1 = p1;
                c2 = p2;
                return;
            }
            if (a <= Epsilon)
            {
                // First segment is basically a point.
                s = F64.C0;
                t = MathHelper.Clamp(f / e, F64.C0, F64.C1);
            }
            else
            {
				Fixed64 c = FixedV3.Dot(d1, r);
                if (e <= Epsilon)
                {
                    // Second segment is basically a point.
                    t = F64.C0;
                    s = MathHelper.Clamp(-c / a, F64.C0, F64.C1);
                }
                else
                {
					Fixed64 b = FixedV3.Dot(d1, d2);
					Fixed64 denom = a * e - b * b;

                    // If segments not parallel, compute closest point on L1 to L2, and
                    // clamp to segment S1. Else pick some s (here .5f)
                    if (denom != F64.C0)
                        s = MathHelper.Clamp((b * f - c * e) / denom, F64.C0, F64.C1);
                    else //Parallel, just use .5f
                        s = F64.C0p5;


                    t = (b * s + f) / e;

                    if (t < F64.C0)
                    {
                        //Closest point is before the segment.
                        t = F64.C0;
                        s = MathHelper.Clamp(-c / a, F64.C0, F64.C1);
                    }
                    else if (t > F64.C1)
                    {
                        //Closest point is after the segment.
                        t = F64.C1;
                        s = MathHelper.Clamp((b - c) / a, F64.C0, F64.C1);
                    }
                }
            }

            FixedV3.Multiply(ref d1, s, out c1);
            FixedV3.Add(ref c1, ref p1, out c1);
            FixedV3.Multiply(ref d2, t, out c2);
            FixedV3.Add(ref c2, ref p2, out c2);
        }


        /// <summary>
        /// Computes closest points c1 and c2 betwen lines p1q1 and p2q2.
        /// </summary>
        /// <param name="p1">First point of first segment.</param>
        /// <param name="q1">Second point of first segment.</param>
        /// <param name="p2">First point of second segment.</param>
        /// <param name="q2">Second point of second segment.</param>
        /// <param name="s">Distance along the line to the point for first segment.</param>
        /// <param name="t">Distance along the line to the point for second segment.</param>
        /// <param name="c1">Closest point on first segment.</param>
        /// <param name="c2">Closest point on second segment.</param>
        public static void GetClosestPointsBetweenLines(ref FixedV3 p1, ref FixedV3 q1, ref FixedV3 p2, ref FixedV3 q2,
                                                           out Fixed64 s, out Fixed64 t, out FixedV3 c1, out FixedV3 c2)
        {
            //Segment direction vectors
            FixedV3 d1;
            FixedV3.Subtract(ref q1, ref p1, out d1);
            FixedV3 d2;
            FixedV3.Subtract(ref q2, ref p2, out d2);
            FixedV3 r;
            FixedV3.Subtract(ref p1, ref p2, out r);
			//distance
			Fixed64 a = d1.LengthSquared();
			Fixed64 e = d2.LengthSquared();
			Fixed64 f;
            FixedV3.Dot(ref d2, ref r, out f);

            if (a <= Epsilon && e <= Epsilon)
            {
                //These segments are more like points.
                s = t = F64.C0;
                c1 = p1;
                c2 = p2;
                return;
            }
            if (a <= Epsilon)
            {
                // First segment is basically a point.
                s = F64.C0;
                t = MathHelper.Clamp(f / e, F64.C0, F64.C1);
            }
            else
            {
				Fixed64 c = FixedV3.Dot(d1, r);
                if (e <= Epsilon)
                {
                    // Second segment is basically a point.
                    t = F64.C0;
                    s = MathHelper.Clamp(-c / a, F64.C0, F64.C1);
                }
                else
                {
					Fixed64 b = FixedV3.Dot(d1, d2);
					Fixed64 denom = a * e - b * b;

                    // If segments not parallel, compute closest point on L1 to L2, and
                    // clamp to segment S1. Else pick some s (here .5f)
                    if (denom != F64.C0)
                        s = (b * f - c * e) / denom;
                    else //Parallel, just use .5f
                        s = F64.C0p5;


                    t = (b * s + f) / e;
                }
            }

            FixedV3.Multiply(ref d1, s, out c1);
            FixedV3.Add(ref c1, ref p1, out c1);
            FixedV3.Multiply(ref d2, t, out c2);
            FixedV3.Add(ref c2, ref p2, out c2);
        }



        #endregion


        #region Point-Plane Tests

        /// <summary>
        /// Determines if vectors o and p are on opposite sides of the plane defined by a, b, and c.
        /// </summary>
        /// <param name="o">First point for comparison.</param>
        /// <param name="p">Second point for comparison.</param>
        /// <param name="a">First vertex of the plane.</param>
        /// <param name="b">Second vertex of plane.</param>
        /// <param name="c">Third vertex of plane.</param>
        /// <returns>Whether or not vectors o and p reside on opposite sides of the plane.</returns>
        public static bool ArePointsOnOppositeSidesOfPlane(ref FixedV3 o, ref FixedV3 p, ref FixedV3 a, ref FixedV3 b, ref FixedV3 c)
        {
            FixedV3 ab, ac, ap, ao;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3.Subtract(ref c, ref a, out ac);
            FixedV3.Subtract(ref p, ref a, out ap);
            FixedV3.Subtract(ref o, ref a, out ao);
            FixedV3 q;
            FixedV3.Cross(ref ab, ref ac, out q);
			Fixed64 signp;
            FixedV3.Dot(ref ap, ref q, out signp);
			Fixed64 signo;
            FixedV3.Dot(ref ao, ref q, out signo);
            if (signp * signo <= F64.C0)
                return true;
            return false;
        }

        /// <summary>
        /// Determines the distance between a point and a plane..
        /// </summary>
        /// <param name="point">Point to project onto plane.</param>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="pointOnPlane">Point located on the plane.</param>
        /// <returns>Distance from the point to the plane.</returns>
        public static Fixed64 GetDistancePointToPlane(ref FixedV3 point, ref FixedV3 normal, ref FixedV3 pointOnPlane)
        {
            FixedV3 offset;
            FixedV3.Subtract(ref point, ref pointOnPlane, out offset);
			Fixed64 dot;
            FixedV3.Dot(ref normal, ref offset, out dot);
            return dot / normal.LengthSquared();
        }

        /// <summary>
        /// Determines the location of the point when projected onto the plane defined by the normal and a point on the plane.
        /// </summary>
        /// <param name="point">Point to project onto plane.</param>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="pointOnPlane">Point located on the plane.</param>
        /// <param name="projectedPoint">Projected location of point onto plane.</param>
        public static void GetPointProjectedOnPlane(ref FixedV3 point, ref FixedV3 normal, ref FixedV3 pointOnPlane, out FixedV3 projectedPoint)
        {
			Fixed64 dot;
            FixedV3.Dot(ref normal, ref point, out dot);
			Fixed64 dot2;
            FixedV3.Dot(ref pointOnPlane, ref normal, out dot2);
			Fixed64 t = (dot - dot2) / normal.LengthSquared();
            FixedV3 multiply;
            FixedV3.Multiply(ref normal, t, out multiply);
            FixedV3.Subtract(ref point, ref multiply, out projectedPoint);
        }

        /// <summary>
        /// Determines if a point is within a set of planes defined by the edges of a triangle.
        /// </summary>
        /// <param name="point">Point for comparison.</param>
        /// <param name="planes">Edge planes.</param>
        /// <param name="centroid">A point known to be inside of the planes.</param>
        /// <returns>Whether or not the point is within the edge planes.</returns>
        public static bool IsPointWithinFaceExtrusion(FixedV3 point, List<BEPUPlane> planes, FixedV3 centroid)
        {
            foreach (BEPUPlane plane in planes)
            {
				Fixed64 centroidPlaneDot;
                plane.DotCoordinate(ref centroid, out centroidPlaneDot);
				Fixed64 pointPlaneDot;
                plane.DotCoordinate(ref point, out pointPlaneDot);
                if (!((centroidPlaneDot <= Epsilon && pointPlaneDot <= Epsilon) || (centroidPlaneDot >= -Epsilon && pointPlaneDot >= -Epsilon)))
                {
                    //Point's NOT the same side of the centroid, so it's 'outside.'
                    return false;
                }
            }
            return true;
        }


        #endregion

        #region Tetrahedron Tests
        //Note: These methods are unused in modern systems, but are kept around for verification.

        /// <summary>
        /// Determines the closest point on a tetrahedron to a provided point p.
        /// </summary>
        /// <param name="a">First vertex of the tetrahedron.</param>
        /// <param name="b">Second vertex of the tetrahedron.</param>
        /// <param name="c">Third vertex of the tetrahedron.</param>
        /// <param name="d">Fourth vertex of the tetrahedron.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="closestPoint">Closest point on the tetrahedron to the point.</param>
        public static void GetClosestPointOnTetrahedronToPoint(ref FixedV3 a, ref FixedV3 b, ref FixedV3 c, ref FixedV3 d, ref FixedV3 p, out FixedV3 closestPoint)
        {
            // Start out assuming point inside all halfspaces, so closest to itself
            closestPoint = p;
            FixedV3 pq;
            FixedV3 q;
			Fixed64 bestSqDist = Fixed64.MaxValue;
            // If point outside face abc then compute closest point on abc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref d, ref a, ref b, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref b, ref c, ref p, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face acd
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref b, ref a, ref c, ref d))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref c, ref d, ref p, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face adb
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref c, ref a, ref d, ref b))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref d, ref b, ref p, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face bdc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref a, ref b, ref d, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref b, ref d, ref c, ref p, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    closestPoint = q;
                }
            }
        }

        /// <summary>
        /// Determines the closest point on a tetrahedron to a provided point p.
        /// </summary>
        /// <param name="a">First vertex of the tetrahedron.</param>
        /// <param name="b">Second vertex of the tetrahedron.</param>
        /// <param name="c">Third vertex of the tetrahedron.</param>
        /// <param name="d">Fourth vertex of the tetrahedron.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point.</param>
        /// <param name="closestPoint">Closest point on the tetrahedron to the point.</param>
        [Obsolete("This method was used for older GJK simplex tests.  If you need simplex tests, consider the PairSimplex class and its variants.")]
        public static void GetClosestPointOnTetrahedronToPoint(ref FixedV3 a, ref FixedV3 b, ref FixedV3 c, ref FixedV3 d, ref FixedV3 p, RawList<FixedV3> subsimplex, out FixedV3 closestPoint)
        {
            // Start out assuming point inside all halfspaces, so closest to itself
            subsimplex.Clear();
            subsimplex.Add(a); //Provides a baseline; if the object is not outside of any planes, then it's inside and the subsimplex is the tetrahedron itself.
            subsimplex.Add(b);
            subsimplex.Add(c);
            subsimplex.Add(d);
            closestPoint = p;
            FixedV3 pq;
            FixedV3 q;
			Fixed64 bestSqDist = Fixed64.MaxValue;
            // If point outside face abc then compute closest point on abc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref d, ref a, ref b, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref b, ref c, ref p, subsimplex, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face acd
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref b, ref a, ref c, ref d))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref c, ref d, ref p, subsimplex, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face adb
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref c, ref a, ref d, ref b))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref d, ref b, ref p, subsimplex, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face bdc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref a, ref b, ref d, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref b, ref d, ref c, ref p, subsimplex, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    closestPoint = q;
                }
            }
        }

        /// <summary>
        /// Determines the closest point on a tetrahedron to a provided point p.
        /// </summary>
        /// <param name="tetrahedron">List of 4 points composing the tetrahedron.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point, enumerated as a = 0, b = 1, c = 2, d = 3.</param>
        /// <param name="baryCoords">Barycentric coordinates of p on the tetrahedron.</param>
        /// <param name="closestPoint">Closest point on the tetrahedron to the point.</param>
        [Obsolete("This method was used for older GJK simplex tests.  If you need simplex tests, consider the PairSimplex class and its variants.")]
        public static void GetClosestPointOnTetrahedronToPoint(RawList<FixedV3> tetrahedron, ref FixedV3 p, RawList<int> subsimplex, RawList<Fixed64> baryCoords, out FixedV3 closestPoint)
        {
            var subsimplexCandidate = CommonResources.GetIntList();
            var baryCoordsCandidate = CommonResources.GetFloatList();
            FixedV3 a = tetrahedron[0];
            FixedV3 b = tetrahedron[1];
            FixedV3 c = tetrahedron[2];
            FixedV3 d = tetrahedron[3];
            closestPoint = p;
            FixedV3 pq;
			Fixed64 bestSqDist = Fixed64.MaxValue;
            subsimplex.Clear();
            subsimplex.Add(0); //Provides a baseline; if the object is not outside of any planes, then it's inside and the subsimplex is the tetrahedron itself.
            subsimplex.Add(1);
            subsimplex.Add(2);
            subsimplex.Add(3);
            baryCoords.Clear();
            FixedV3 q;
            bool baryCoordsFound = false;

            // If point outside face abc then compute closest point on abc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref d, ref a, ref b, ref c))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 0, 1, 2, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.LengthSquared();
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            // Repeat test for face acd
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref b, ref a, ref c, ref d))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 0, 2, 3, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.LengthSquared();
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            // Repeat test for face adb
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref c, ref a, ref d, ref b))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 0, 3, 1, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.LengthSquared();
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            // Repeat test for face bdc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref a, ref b, ref d, ref c))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 1, 3, 2, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                FixedV3.Subtract(ref q, ref p, out pq);
				Fixed64 sqDist = pq.LengthSquared();
                if (sqDist < bestSqDist)
                {
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            if (!baryCoordsFound)
            {
				//subsimplex is the entire tetrahedron, can only occur when objects intersect!  Determinants of each of the tetrahedrons based on triangles composing the sides and the point itself.
				//This is basically computing the volume of parallelepipeds (triple scalar product).
				//Could be quicker just to do it directly.
				Fixed64 abcd = (new BEPUMatrix(tetrahedron[0].X, tetrahedron[0].Y, tetrahedron[0].Z, F64.C1,
                                         tetrahedron[1].X, tetrahedron[1].Y, tetrahedron[1].Z, F64.C1,
                                         tetrahedron[2].X, tetrahedron[2].Y, tetrahedron[2].Z, F64.C1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, F64.C1)).Determinant();
				Fixed64 pbcd = (new BEPUMatrix(p.X, p.Y, p.Z, F64.C1,
                                         tetrahedron[1].X, tetrahedron[1].Y, tetrahedron[1].Z, F64.C1,
                                         tetrahedron[2].X, tetrahedron[2].Y, tetrahedron[2].Z, F64.C1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, F64.C1)).Determinant();
				Fixed64 apcd = (new BEPUMatrix(tetrahedron[0].X, tetrahedron[0].Y, tetrahedron[0].Z, F64.C1,
                                         p.X, p.Y, p.Z, F64.C1,
                                         tetrahedron[2].X, tetrahedron[2].Y, tetrahedron[2].Z, F64.C1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, F64.C1)).Determinant();
				Fixed64 abpd = (new BEPUMatrix(tetrahedron[0].X, tetrahedron[0].Y, tetrahedron[0].Z, F64.C1,
                                         tetrahedron[1].X, tetrahedron[1].Y, tetrahedron[1].Z, F64.C1,
                                         p.X, p.Y, p.Z, F64.C1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, F64.C1)).Determinant();
                abcd = F64.C1 / abcd;
                baryCoords.Add(pbcd * abcd); //u
                baryCoords.Add(apcd * abcd); //v
                baryCoords.Add(abpd * abcd); //w
                baryCoords.Add(F64.C1 - baryCoords[0] - baryCoords[1] - baryCoords[2]); //x = 1-u-v-w
            }
            CommonResources.GiveBack(subsimplexCandidate);
            CommonResources.GiveBack(baryCoordsCandidate);
        }

        #endregion





        #region Miscellaneous

        ///<summary>
        /// Tests a ray against a sphere.
        ///</summary>
        ///<param name="ray">Ray to test.</param>
        ///<param name="spherePosition">Position of the sphere.</param>
        ///<param name="radius">Radius of the sphere.</param>
        ///<param name="maximumLength">Maximum length of the ray in units of the ray direction's length.</param>
        ///<param name="hit">Hit data of the ray, if any.</param>
        ///<returns>Whether or not the ray hits the sphere.</returns>
        public static bool RayCastSphere(ref BEPURay ray, ref FixedV3 spherePosition, Fixed64 radius, Fixed64 maximumLength, out RayHit hit)
        {
            FixedV3 normalizedDirection;
			Fixed64 length = ray.Direction.Length();
            FixedV3.Divide(ref ray.Direction, length, out normalizedDirection);
            maximumLength *= length;
            hit = new RayHit();
            FixedV3 m;
            FixedV3.Subtract(ref ray.Position, ref spherePosition, out m);
			Fixed64 b = FixedV3.Dot(m, normalizedDirection);
			Fixed64 c = m.LengthSquared() - radius * radius;

            if (c > F64.C0 && b > F64.C0)
                return false;
			Fixed64 discriminant = b * b - c;
            if (discriminant < F64.C0)
                return false;

            hit.T = -b - Fixed64.Sqrt(discriminant);
            if (hit.T < F64.C0)
                hit.T = F64.C0;
            if (hit.T > maximumLength)
                return false;
            hit.T /= length;
            FixedV3.Multiply(ref normalizedDirection, hit.T, out hit.Location);
            FixedV3.Add(ref hit.Location, ref ray.Position, out hit.Location);
            FixedV3.Subtract(ref hit.Location, ref spherePosition, out hit.Normal);
            hit.Normal.Normalize();
            return true;
        }


        /// <summary>
        /// Computes the velocity of a point as if it were attached to an object with the given center and velocity.
        /// </summary>
        /// <param name="point">Point to compute the velocity of.</param>
        /// <param name="center">Center of the object to which the point is attached.</param>
        /// <param name="linearVelocity">Linear velocity of the object.</param>
        /// <param name="angularVelocity">Angular velocity of the object.</param>
        /// <param name="velocity">Velocity of the point.</param>
        public static void GetVelocityOfPoint(ref FixedV3 point, ref FixedV3 center, ref FixedV3 linearVelocity, ref FixedV3 angularVelocity, out FixedV3 velocity)
        {
            FixedV3 offset;
            FixedV3.Subtract(ref point, ref center, out offset);
            FixedV3.Cross(ref angularVelocity, ref offset, out velocity);
            FixedV3.Add(ref velocity, ref linearVelocity, out velocity);
        }

        /// <summary>
        /// Computes the velocity of a point as if it were attached to an object with the given center and velocity.
        /// </summary>
        /// <param name="point">Point to compute the velocity of.</param>
        /// <param name="center">Center of the object to which the point is attached.</param>
        /// <param name="linearVelocity">Linear velocity of the object.</param>
        /// <param name="angularVelocity">Angular velocity of the object.</param>
        /// <returns>Velocity of the point.</returns>
        public static FixedV3 GetVelocityOfPoint(FixedV3 point, FixedV3 center, FixedV3 linearVelocity, FixedV3 angularVelocity)
        {
            FixedV3 toReturn;
            GetVelocityOfPoint(ref point, ref center, ref linearVelocity, ref angularVelocity, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Expands a bounding box by the given sweep.
        /// </summary>
        /// <param name="boundingBox">Bounding box to expand.</param>
        /// <param name="sweep">Sweep to expand the bounding box with.</param>
        public static void ExpandBoundingBox(ref BoundingBox boundingBox, ref FixedV3 sweep)
        {
            if (sweep.X > F64.C0)
                boundingBox.Max.X += sweep.X;
            else
                boundingBox.Min.X += sweep.X;

            if (sweep.Y > F64.C0)
                boundingBox.Max.Y += sweep.Y;
            else
                boundingBox.Min.Y += sweep.Y;

            if (sweep.Z > F64.C0)
                boundingBox.Max.Z += sweep.Z;
            else
                boundingBox.Min.Z += sweep.Z;
        }

        /// <summary>
        /// Computes the bounding box of three points.
        /// </summary>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="aabb">Bounding box of the triangle.</param>
        public static void GetTriangleBoundingBox(ref FixedV3 a, ref FixedV3 b, ref FixedV3 c, out BoundingBox aabb)
        {
#if !WINDOWS
            aabb = new BoundingBox();
#endif
            //X axis
            if (a.X > b.X && a.X > c.X)
            {
                //A is max
                aabb.Max.X = a.X;
                aabb.Min.X = b.X > c.X ? c.X : b.X;
            }
            else if (b.X > c.X)
            {
                //B is max
                aabb.Max.X = b.X;
                aabb.Min.X = a.X > c.X ? c.X : a.X;
            }
            else
            {
                //C is max
                aabb.Max.X = c.X;
                aabb.Min.X = a.X > b.X ? b.X : a.X;
            }
            //Y axis
            if (a.Y > b.Y && a.Y > c.Y)
            {
                //A is max
                aabb.Max.Y = a.Y;
                aabb.Min.Y = b.Y > c.Y ? c.Y : b.Y;
            }
            else if (b.Y > c.Y)
            {
                //B is max
                aabb.Max.Y = b.Y;
                aabb.Min.Y = a.Y > c.Y ? c.Y : a.Y;
            }
            else
            {
                //C is max
                aabb.Max.Y = c.Y;
                aabb.Min.Y = a.Y > b.Y ? b.Y : a.Y;
            }
            //Z axis
            if (a.Z > b.Z && a.Z > c.Z)
            {
                //A is max
                aabb.Max.Z = a.Z;
                aabb.Min.Z = b.Z > c.Z ? c.Z : b.Z;
            }
            else if (b.Z > c.Z)
            {
                //B is max
                aabb.Max.Z = b.Z;
                aabb.Min.Z = a.Z > c.Z ? c.Z : a.Z;
            }
            else
            {
                //C is max
                aabb.Max.Z = c.Z;
                aabb.Min.Z = a.Z > b.Z ? b.Z : a.Z;
            }
        }






        /// <summary>
        /// Updates the quaternion using RK4 integration.
        /// </summary>
        /// <param name="q">Quaternion to update.</param>
        /// <param name="localInertiaTensorInverse">Local-space inertia tensor of the object being updated.</param>
        /// <param name="angularMomentum">Angular momentum of the object.</param>
        /// <param name="dt">Time since last frame, in seconds.</param>
        /// <param name="newOrientation">New orientation quaternion.</param>
        public static void UpdateOrientationRK4(ref FixedQuaternion q, ref BEPUMatrix3x3 localInertiaTensorInverse, ref FixedV3 angularMomentum, Fixed64 dt, out FixedQuaternion newOrientation)
        {
            //TODO: This is a little goofy
            //Quaternion diff = differentiateQuaternion(ref q, ref localInertiaTensorInverse, ref angularMomentum);
            FixedQuaternion d1;
            DifferentiateQuaternion(ref q, ref localInertiaTensorInverse, ref angularMomentum, out d1);
            FixedQuaternion s2;
            FixedQuaternion.Multiply(ref d1, dt * F64.C0p5, out s2);
            FixedQuaternion.Add(ref q, ref s2, out s2);

            FixedQuaternion d2;
            DifferentiateQuaternion(ref s2, ref localInertiaTensorInverse, ref angularMomentum, out d2);
            FixedQuaternion s3;
            FixedQuaternion.Multiply(ref d2, dt * F64.C0p5, out s3);
            FixedQuaternion.Add(ref q, ref s3, out s3);

            FixedQuaternion d3;
            DifferentiateQuaternion(ref s3, ref localInertiaTensorInverse, ref angularMomentum, out d3);
            FixedQuaternion s4;
            FixedQuaternion.Multiply(ref d3, dt, out s4);
            FixedQuaternion.Add(ref q, ref s4, out s4);

            FixedQuaternion d4;
            DifferentiateQuaternion(ref s4, ref localInertiaTensorInverse, ref angularMomentum, out d4);

            FixedQuaternion.Multiply(ref d1, dt / F64.C6, out d1);
            FixedQuaternion.Multiply(ref d2, dt / F64.C3, out d2);
            FixedQuaternion.Multiply(ref d3, dt / F64.C3, out d3);
            FixedQuaternion.Multiply(ref d4, dt / F64.C6, out d4);
            FixedQuaternion added;
            FixedQuaternion.Add(ref q, ref d1, out added);
            FixedQuaternion.Add(ref added, ref d2, out added);
            FixedQuaternion.Add(ref added, ref d3, out added);
            FixedQuaternion.Add(ref added, ref d4, out added);
            FixedQuaternion.Normalize(ref added, out newOrientation);
        }


        /// <summary>
        /// Finds the change in the rotation state quaternion provided the local inertia tensor and angular velocity.
        /// </summary>
        /// <param name="orientation">Orienatation of the object.</param>
        /// <param name="localInertiaTensorInverse">Local-space inertia tensor of the object being updated.</param>
        /// <param name="angularMomentum">Angular momentum of the object.</param>
        ///  <param name="orientationChange">Change in quaternion.</param>
        public static void DifferentiateQuaternion(ref FixedQuaternion orientation, ref BEPUMatrix3x3 localInertiaTensorInverse, ref FixedV3 angularMomentum, out FixedQuaternion orientationChange)
        {
            FixedQuaternion normalizedOrientation;
            FixedQuaternion.Normalize(ref orientation, out normalizedOrientation);
            BEPUMatrix3x3 tempRotMat;
            BEPUMatrix3x3.CreateFromQuaternion(ref normalizedOrientation, out tempRotMat);
            BEPUMatrix3x3 tempInertiaTensorInverse;
            BEPUMatrix3x3.MultiplyTransposed(ref tempRotMat, ref localInertiaTensorInverse, out tempInertiaTensorInverse);
            BEPUMatrix3x3.Multiply(ref tempInertiaTensorInverse, ref tempRotMat, out tempInertiaTensorInverse);
            FixedV3 halfspin;
            BEPUMatrix3x3.Transform(ref angularMomentum, ref tempInertiaTensorInverse, out halfspin);
            FixedV3.Multiply(ref halfspin, F64.C0p5, out halfspin);
            var halfspinQuaternion = new FixedQuaternion(halfspin.X, halfspin.Y, halfspin.Z, F64.C0);
            FixedQuaternion.Multiply(ref halfspinQuaternion, ref normalizedOrientation, out orientationChange);
        }


        /// <summary>
        /// Gets the barycentric coordinates of the point with respect to a triangle's vertices.
        /// </summary>
        /// <param name="p">Point to compute the barycentric coordinates of.</param>
        /// <param name="a">First vertex in the triangle.</param>
        /// <param name="b">Second vertex in the triangle.</param>
        /// <param name="c">Third vertex in the triangle.</param>
        /// <param name="aWeight">Weight of the first vertex.</param>
        /// <param name="bWeight">Weight of the second vertex.</param>
        /// <param name="cWeight">Weight of the third vertex.</param>
        public static void GetBarycentricCoordinates(ref FixedV3 p, ref FixedV3 a, ref FixedV3 b, ref FixedV3 c, out Fixed64 aWeight, out Fixed64 bWeight, out Fixed64 cWeight)
        {
            FixedV3 ab, ac;
            FixedV3.Subtract(ref b, ref a, out ab);
            FixedV3.Subtract(ref c, ref a, out ac);
            FixedV3 triangleNormal;
            FixedV3.Cross(ref ab, ref ac, out triangleNormal);
            Fixed64 x = triangleNormal.X < F64.C0 ? -triangleNormal.X : triangleNormal.X;
            Fixed64 y = triangleNormal.Y < F64.C0 ? -triangleNormal.Y : triangleNormal.Y;
            Fixed64 z = triangleNormal.Z < F64.C0 ? -triangleNormal.Z : triangleNormal.Z;

            Fixed64 numeratorU, numeratorV, denominator;
            if (x >= y && x >= z)
            {
                //The projection of the triangle on the YZ plane is the largest.
                numeratorU = (p.Y - b.Y) * (b.Z - c.Z) - (b.Y - c.Y) * (p.Z - b.Z); //PBC
                numeratorV = (p.Y - c.Y) * (c.Z - a.Z) - (c.Y - a.Y) * (p.Z - c.Z); //PCA
                denominator = triangleNormal.X;
            }
            else if (y >= z)
            {
                //The projection of the triangle on the XZ plane is the largest.
                numeratorU = (p.X - b.X) * (b.Z - c.Z) - (b.X - c.X) * (p.Z - b.Z); //PBC
                numeratorV = (p.X - c.X) * (c.Z - a.Z) - (c.X - a.X) * (p.Z - c.Z); //PCA
                denominator = -triangleNormal.Y;
            }
            else
            {
                //The projection of the triangle on the XY plane is the largest.
                numeratorU = (p.X - b.X) * (b.Y - c.Y) - (b.X - c.X) * (p.Y - b.Y); //PBC
                numeratorV = (p.X - c.X) * (c.Y - a.Y) - (c.X - a.X) * (p.Y - c.Y); //PCA
                denominator = triangleNormal.Z;
            }

            if (denominator < F64.Cm1em9 || denominator > F64.C1em9)
            {
                denominator = F64.C1 / denominator;
                aWeight = numeratorU * denominator;
                bWeight = numeratorV * denominator;
                cWeight = F64.C1 - aWeight - bWeight;
            }
            else
            {
				//It seems to be a degenerate triangle.
				//In that case, pick one of the closest vertices.
				//MOST of the time, this will happen when the vertices
				//are all very close together (all three points form a single point).
				//Sometimes, though, it could be that it's more of a line.
				//If it's a little inefficient, don't worry- this is a corner case anyway.

				Fixed64 distance1, distance2, distance3;
                FixedV3.DistanceSquared(ref p, ref a, out distance1);
                FixedV3.DistanceSquared(ref p, ref b, out distance2);
                FixedV3.DistanceSquared(ref p, ref c, out distance3);
                if (distance1 < distance2 && distance1 < distance3)
                {
                    aWeight = F64.C1;
                    bWeight = F64.C0;
                    cWeight = F64.C0;
                }
                else if (distance2 < distance3)
                {
                    aWeight = F64.C0;
                    bWeight = F64.C1;
                    cWeight = F64.C0;
                }
                else
                {
                    aWeight = F64.C0;
                    bWeight = F64.C0;
                    cWeight = F64.C1;
                }
            }


        }




        #endregion
    }
}
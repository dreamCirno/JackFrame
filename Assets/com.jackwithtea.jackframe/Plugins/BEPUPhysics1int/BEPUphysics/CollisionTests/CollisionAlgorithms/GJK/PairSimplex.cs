﻿using BEPUPhysics1int.CollisionShapes.ConvexShapes;
 
using BEPUPhysics1int;
using FixMath.NET;
using System.Diagnostics;

namespace BEPUPhysics1int.CollisionTests.CollisionAlgorithms.GJK
{
    
    ///<summary>
    /// Defines the state of a simplex.
    ///</summary>
    public enum SimplexState : byte
    {
        Empty,
        Point,
        Segment,
        Triangle,
        Tetrahedron
    }

    ///<summary>
    /// Stored simplex used to warmstart closest point GJK runs.
    ///</summary>
    public struct CachedSimplex
    {
        //public CachedSimplex(ConvexShape shapeA, ConvexShape shapeB, ref RigidTransform transformA, ref RigidTransform transformB)
        //{
        //    RigidTransform localTransformB;
        //    MinkowskiToolbox.GetLocalTransform(ref transformA, ref transformB, out localTransformB);
        //    LocalSimplexA = new ContributingShapeSimplex();
        //    LocalSimplexB = new ContributingShapeSimplex();

        //    State = SimplexState.Point;
        //    return;
        //    shapeA.GetLocalExtremePointWithoutMargin(ref localTransformB.Position, out LocalSimplexA.A);
        //    Vector3 direction;
        //    Vector3.Negate(ref localTransformB.Position, out direction);
        //    Quaternion conjugate;
        //    Quaternion.Conjugate(ref localTransformB.Orientation, out conjugate);
        //    Vector3.Transform(ref direction, ref conjugate, out direction);
        //    shapeB.GetLocalExtremePointWithoutMargin(ref direction, out LocalSimplexB.A);
        //}

        //public CachedSimplex(ConvexShape shapeA, ConvexShape shapeB, ref RigidTransform localTransformB)
        //{
        //    LocalSimplexA = new ContributingShapeSimplex();
        //    LocalSimplexB = new ContributingShapeSimplex();

        //    State = SimplexState.Point;
        //    return;
        //    shapeA.GetLocalExtremePointWithoutMargin(ref localTransformB.Position, out LocalSimplexA.A);
        //    Vector3 direction;
        //    Vector3.Negate(ref localTransformB.Position, out direction);
        //    Quaternion conjugate;
        //    Quaternion.Conjugate(ref localTransformB.Orientation, out conjugate);
        //    Vector3.Transform(ref direction, ref conjugate, out direction);
        //    shapeB.GetLocalExtremePointWithoutMargin(ref direction, out LocalSimplexB.A);
        //}


        ///<summary>
        /// Simplex in the local space of shape A.
        ///</summary>
        public ContributingShapeSimplex LocalSimplexA;

        ///<summary>
        /// Simplex in the local space of shape B.
        ///</summary>
        public ContributingShapeSimplex LocalSimplexB;
        
        /// <summary>
        /// State of the simplex at the termination of the last GJK run.
        /// </summary>
        public SimplexState State;
    }

    ///<summary>
    /// List of points composing a shape's contributions to a simplex.
    ///</summary>
    public struct ContributingShapeSimplex
    {
        public FixedV3 A;
        public FixedV3 B;
        public FixedV3 C;
        public FixedV3 D;
    }

    ///<summary>
    /// GJK simplex used to support closest point tests with warmstarting.
    ///</summary>
    public struct PairSimplex
    {
        ///<summary>
        /// The baseline amount that a GJK iteration must progress through to avoid exiting.
        /// Defaults to 1e-8f.
        ///</summary>
        public static Fixed64 ProgressionEpsilon = (Fixed64)1e-8m;
        /// <summary>
        /// The baseline amount that an iteration must converge with its distance to avoid exiting.
        /// Defaults to 1e-7f.
        /// </summary>
        public static Fixed64 DistanceConvergenceEpsilon = (Fixed64)1e-7m;

        ///<summary>
        /// Simplex as viewed from the local space of A.
        ///</summary>
        public ContributingShapeSimplex SimplexA;

        ///<summary>
        /// Simplex as viewed from the local space of B.
        ///</summary>
        public ContributingShapeSimplex SimplexB;

        public FixedV3 A;
        public FixedV3 B;
        public FixedV3 C;
        public FixedV3 D;
        public SimplexState State;
        /// <summary>
        /// Weight of vertex A.
        /// </summary>
        public Fixed64 U;
        /// <summary>
        /// Weight of vertex B.
        /// </summary>
        public Fixed64 V;
        /// <summary>
        /// Weight of vertex C.
        /// </summary>
        public Fixed64 W;
        /// <summary>
        /// Transform of the second shape in the first shape's local space.
        /// </summary>
        public RigidTransform LocalTransformB;


        private PairSimplex(ref RigidTransform localTransformB)
        {
            //This isn't a very good approach since the transform position is not guaranteed to be within the object.  Would have to use the GetNewSimplexPoint to make it valid.
            previousDistanceToClosest = Fixed64.MaxValue;
            errorTolerance = F64.C0;
            LocalTransformB = localTransformB;
            //Warm up the simplex using the centroids.
            //Could also use the GetNewSimplexPoint if it had a Empty case, but test before choosing.
            State = SimplexState.Point;
            SimplexA = new ContributingShapeSimplex();
            SimplexB = new ContributingShapeSimplex {A = localTransformB.Position};
            //minkowski space support = shapeA-shapeB = 0,0,0 - positionB
            FixedV3.Negate(ref localTransformB.Position, out A);
            B = new FixedV3();
            C = new FixedV3();
            D = new FixedV3();
            U = F64.C0;
            V = F64.C0;
            W = F64.C0;
        }

        ///<summary>
        /// Constructs a new pair simplex.
        ///</summary>
        ///<param name="cachedSimplex">Cached simplex to use to warmstart the simplex.</param>
        ///<param name="localTransformB">Transform of shape B in the local space of A.</param>
        public PairSimplex(ref CachedSimplex cachedSimplex, ref RigidTransform localTransformB)
        {
            //NOTE:
            //USING A CACHED SIMPLEX INVALIDATES ASSUMPTIONS THAT ALLOW SIMPLEX CASES TO BE IGNORED!
            //To get those assumptions back, either DO NOT USE CACHED SIMPLEXES, or 
            //VERIFY THE SIMPLEXES.
            //-A point requires no verification.
            //-A segment needs verification that the origin is in front of A in the direction of B.
            //-A triangle needs verification that the origin is within the edge planes and in the direction of C.
            //-A tetrahedron needs verification that the origin is within the edge planes of triangle ABC and is in the direction of D.

            //This simplex implementation will not ignore any cases, so we can warm start safely with one problem.
            //Due to relative movement, the simplex may become degenerate.  Edges could become points, etc.
            //Some protections are built into the simplex cases, but keep an eye out for issues.
            //Most dangerous degeneracy seen so far is tetrahedron.  It fails to find any points on opposing sides due to numerical problems and returns intersection.


            previousDistanceToClosest = Fixed64.MaxValue;
            errorTolerance = F64.C0;
            LocalTransformB = localTransformB;

            //Transform the SimplexB into the working space of the simplex and compute the working space simplex.
            State = cachedSimplex.State;
            SimplexA = cachedSimplex.LocalSimplexA;
            SimplexB = new ContributingShapeSimplex();
            U = F64.C0;
            V = F64.C0;
            W = F64.C0;
            switch (State)
            {
                case SimplexState.Point:
                    FixedQuaternion.Transform(ref cachedSimplex.LocalSimplexB.A, ref LocalTransformB.Orientation, out SimplexB.A);
                    FixedV3.Add(ref SimplexB.A, ref LocalTransformB.Position, out SimplexB.A);

                    FixedV3.Subtract(ref SimplexA.A, ref SimplexB.A, out A);
                    B = new FixedV3();
                    C = new FixedV3();
                    D = new FixedV3();
                    break;
                case SimplexState.Segment:
                    BEPUMatrix3x3 transform;
                    BEPUMatrix3x3.CreateFromQuaternion(ref localTransformB.Orientation, out transform);
                    BEPUMatrix3x3.Transform(ref cachedSimplex.LocalSimplexB.A, ref transform, out SimplexB.A);
                    BEPUMatrix3x3.Transform(ref cachedSimplex.LocalSimplexB.B, ref transform, out SimplexB.B);
                    FixedV3.Add(ref SimplexB.A, ref LocalTransformB.Position, out SimplexB.A);
                    FixedV3.Add(ref SimplexB.B, ref LocalTransformB.Position, out SimplexB.B);

                    FixedV3.Subtract(ref SimplexA.A, ref SimplexB.A, out A);
                    FixedV3.Subtract(ref SimplexA.B, ref SimplexB.B, out B);
                    C = new FixedV3();
                    D = new FixedV3();

                    ////Test for degeneracy.
                    //Fix64 edgeLengthAB;
                    //Vector3.DistanceSquared(ref A, ref B, out edgeLengthAB);
                    //if (edgeLengthAB < Toolbox.Epsilon)
                    //    State = SimplexState.Point;

                    break;
                case SimplexState.Triangle:
                    BEPUMatrix3x3.CreateFromQuaternion(ref localTransformB.Orientation, out transform);
                    BEPUMatrix3x3.Transform(ref cachedSimplex.LocalSimplexB.A, ref transform, out SimplexB.A);
                    BEPUMatrix3x3.Transform(ref cachedSimplex.LocalSimplexB.B, ref transform, out SimplexB.B);
                    BEPUMatrix3x3.Transform(ref cachedSimplex.LocalSimplexB.C, ref transform, out SimplexB.C);
                    FixedV3.Add(ref SimplexB.A, ref LocalTransformB.Position, out SimplexB.A);
                    FixedV3.Add(ref SimplexB.B, ref LocalTransformB.Position, out SimplexB.B);
                    FixedV3.Add(ref SimplexB.C, ref LocalTransformB.Position, out SimplexB.C);

                    FixedV3.Subtract(ref SimplexA.A, ref SimplexB.A, out A);
                    FixedV3.Subtract(ref SimplexA.B, ref SimplexB.B, out B);
                    FixedV3.Subtract(ref SimplexA.C, ref SimplexB.C, out C);
                    D = new FixedV3();

                    ////Test for degeneracy.
                    //Vector3 AB, AC;
                    //Vector3.Subtract(ref B, ref A, out AB);
                    //Vector3.Subtract(ref C, ref A, out AC);
                    //Vector3 cross;
                    //Vector3.Cross(ref AB, ref AC, out cross);
                    ////If the area is small compared to a tolerance (adjusted by the partial perimeter), it's degenerate.
                    //if (cross.LengthSquared() < Toolbox.BigEpsilon * (AB.LengthSquared() + AC.LengthSquared()))
                    //    State = SimplexState.Point;


                    break;
                case SimplexState.Tetrahedron:
                    BEPUMatrix3x3.CreateFromQuaternion(ref localTransformB.Orientation, out transform);
                    BEPUMatrix3x3.Transform(ref cachedSimplex.LocalSimplexB.A, ref transform, out SimplexB.A);
                    BEPUMatrix3x3.Transform(ref cachedSimplex.LocalSimplexB.B, ref transform, out SimplexB.B);
                    BEPUMatrix3x3.Transform(ref cachedSimplex.LocalSimplexB.C, ref transform, out SimplexB.C);
                    BEPUMatrix3x3.Transform(ref cachedSimplex.LocalSimplexB.D, ref transform, out SimplexB.D);
                    FixedV3.Add(ref SimplexB.A, ref LocalTransformB.Position, out SimplexB.A);
                    FixedV3.Add(ref SimplexB.B, ref LocalTransformB.Position, out SimplexB.B);
                    FixedV3.Add(ref SimplexB.C, ref LocalTransformB.Position, out SimplexB.C);
                    FixedV3.Add(ref SimplexB.D, ref LocalTransformB.Position, out SimplexB.D);

                    FixedV3.Subtract(ref SimplexA.A, ref SimplexB.A, out A);
                    FixedV3.Subtract(ref SimplexA.B, ref SimplexB.B, out B);
                    FixedV3.Subtract(ref SimplexA.C, ref SimplexB.C, out C);
                    FixedV3.Subtract(ref SimplexA.D, ref SimplexB.D, out D);

                    ////Test for degeneracy.
                    //Vector3 AD;
                    //Vector3.Subtract(ref B, ref A, out AB);
                    //Vector3.Subtract(ref C, ref A, out AC);
                    //Vector3.Subtract(ref D, ref A, out AD);
                    //Vector3.Cross(ref AB, ref AC, out cross);
                    //Fix64 volume;
                    //Vector3.Dot(ref cross, ref AD, out volume);

                    ////Volume is small compared to partial 'perimeter.'
                    //if (volume < Toolbox.BigEpsilon * (AB.LengthSquared() + AC.LengthSquared() + AD.LengthSquared()))
                    //    State = SimplexState.Point;
                    break;
                default:
                    A = new FixedV3();
                    B = new FixedV3();
                    C = new FixedV3();
                    D = new FixedV3();
                    break;
            }
        }

        ///<summary>
        /// Updates the cached simplex with the latest run's results.
        ///</summary>
        ///<param name="simplex">Simplex to update.</param>
        public void UpdateCachedSimplex(ref CachedSimplex simplex)
        {
            simplex.LocalSimplexA = SimplexA;
            switch (State)
            {
                case SimplexState.Point:
                    FixedV3.Subtract(ref SimplexB.A, ref LocalTransformB.Position, out simplex.LocalSimplexB.A);
                    FixedQuaternion conjugate;
                    FixedQuaternion.Conjugate(ref LocalTransformB.Orientation, out conjugate);
                    FixedQuaternion.Transform(ref simplex.LocalSimplexB.A, ref conjugate, out simplex.LocalSimplexB.A);
                    break;
                case SimplexState.Segment:
                    FixedV3.Subtract(ref SimplexB.A, ref LocalTransformB.Position, out simplex.LocalSimplexB.A);
                    FixedV3.Subtract(ref SimplexB.B, ref LocalTransformB.Position, out simplex.LocalSimplexB.B);

                    BEPUMatrix3x3 transform;
                    BEPUMatrix3x3.CreateFromQuaternion(ref LocalTransformB.Orientation, out transform);
                    BEPUMatrix3x3.TransformTranspose(ref simplex.LocalSimplexB.A, ref transform, out simplex.LocalSimplexB.A);
                    BEPUMatrix3x3.TransformTranspose(ref simplex.LocalSimplexB.B, ref transform, out simplex.LocalSimplexB.B);
                    break;
                case SimplexState.Triangle:
                    FixedV3.Subtract(ref SimplexB.A, ref LocalTransformB.Position, out simplex.LocalSimplexB.A);
                    FixedV3.Subtract(ref SimplexB.B, ref LocalTransformB.Position, out simplex.LocalSimplexB.B);
                    FixedV3.Subtract(ref SimplexB.C, ref LocalTransformB.Position, out simplex.LocalSimplexB.C);

                    BEPUMatrix3x3.CreateFromQuaternion(ref LocalTransformB.Orientation, out transform);
                    BEPUMatrix3x3.TransformTranspose(ref simplex.LocalSimplexB.A, ref transform, out simplex.LocalSimplexB.A);
                    BEPUMatrix3x3.TransformTranspose(ref simplex.LocalSimplexB.B, ref transform, out simplex.LocalSimplexB.B);
                    BEPUMatrix3x3.TransformTranspose(ref simplex.LocalSimplexB.C, ref transform, out simplex.LocalSimplexB.C);
                    break;
                case SimplexState.Tetrahedron:
                    FixedV3.Subtract(ref SimplexB.A, ref LocalTransformB.Position, out simplex.LocalSimplexB.A);
                    FixedV3.Subtract(ref SimplexB.B, ref LocalTransformB.Position, out simplex.LocalSimplexB.B);
                    FixedV3.Subtract(ref SimplexB.C, ref LocalTransformB.Position, out simplex.LocalSimplexB.C);
                    FixedV3.Subtract(ref SimplexB.D, ref LocalTransformB.Position, out simplex.LocalSimplexB.D);

                    BEPUMatrix3x3.CreateFromQuaternion(ref LocalTransformB.Orientation, out transform);
                    BEPUMatrix3x3.TransformTranspose(ref simplex.LocalSimplexB.A, ref transform, out simplex.LocalSimplexB.A);
                    BEPUMatrix3x3.TransformTranspose(ref simplex.LocalSimplexB.B, ref transform, out simplex.LocalSimplexB.B);
                    BEPUMatrix3x3.TransformTranspose(ref simplex.LocalSimplexB.C, ref transform, out simplex.LocalSimplexB.C);
                    BEPUMatrix3x3.TransformTranspose(ref simplex.LocalSimplexB.D, ref transform, out simplex.LocalSimplexB.D);
                    break;
            }
            simplex.State = State;
        }

        ///<summary>
        /// Gets the point on the simplex closest to the origin.
        ///</summary>
        ///<param name="point">Point closest to the origin.</param>
        ///<returns>Whether or not the simplex encloses the origin.</returns>
        public bool GetPointClosestToOrigin(out FixedV3 point)
        {
            //This method finds the closest point on the simplex to the origin.
            //Barycentric coordinates are assigned to the MinimumNormCoordinates as necessary to perform the inclusion calculation.
            //If the simplex is a tetrahedron and found to be overlapping the origin, the function returns true to tell the caller to terminate.
            //Elements of the simplex that are not used to determine the point of minimum norm are removed from the simplex.

            switch (State)
            {

                case SimplexState.Point:
                    point = A;
                    U = F64.C1;
                    break;
                case SimplexState.Segment:
                    GetPointOnSegmentClosestToOrigin(out point);
                    break;
                case SimplexState.Triangle:
                    GetPointOnTriangleClosestToOrigin(out point);
                    break;
                case SimplexState.Tetrahedron:
                    return GetPointOnTetrahedronClosestToOrigin(out point);
                default:
                    point = Toolbox.ZeroVector;
                    break;


            }
            return false;
        }


        ///<summary>
        /// Gets the point on the segment closest to the origin.
        ///</summary>
        ///<param name="point">Point closest to origin.</param>
        public void GetPointOnSegmentClosestToOrigin(out FixedV3 point)
        {
            FixedV3 segmentDisplacement;
            FixedV3.Subtract(ref B, ref A, out segmentDisplacement);

            Fixed64 dotA;
            FixedV3.Dot(ref segmentDisplacement, ref A, out dotA);
            if (dotA > F64.C0)
            {
                //'Behind' segment.  This can't happen in a boolean version,
                //but with closest points warmstarting or raycasts, it will.
                State = SimplexState.Point;

                U = F64.C1;
                point = A;
                return;
            }
            Fixed64 dotB;
            FixedV3.Dot(ref segmentDisplacement, ref B, out dotB);
            if (dotB > F64.C0)
            {
                //Inside segment.
                U = dotB / segmentDisplacement.LengthSquared();
                V = F64.C1 - U;
                FixedV3.Multiply(ref segmentDisplacement, V, out point);
                FixedV3.Add(ref point, ref A, out point);
                return;

            }

            //It should be possible in the warmstarted closest point calculation/raycasting to be outside B.
            //It is not possible in a 'boolean' GJK, where it early outs as soon as a separating axis is found.

            //Outside B.
            //Remove current A; we're becoming a point.
            A = B;
            SimplexA.A = SimplexA.B;
            SimplexB.A = SimplexB.B;
            State = SimplexState.Point;

            U = F64.C1;
            point = A;

        }

        ///<summary>
        /// Gets the point on the triangle closest to the origin.
        ///</summary>
        ///<param name="point">Point closest to origin.</param>
        public void GetPointOnTriangleClosestToOrigin(out FixedV3 point)
        {
            FixedV3 ab, ac;
            FixedV3.Subtract(ref B, ref A, out ab);
            FixedV3.Subtract(ref C, ref A, out ac);
            //The point we are comparing against the triangle is 0,0,0, so instead of storing an "A->P" vector,
            //just use -A.
            //Same for B->P, C->P...

            //Check to see if it's outside A.
            //TODO: Note that in a boolean-style GJK, it shouldn't be possible to be outside A.
            Fixed64 AdotAB, AdotAC;
            FixedV3.Dot(ref ab, ref A, out AdotAB);
            FixedV3.Dot(ref ac, ref A, out AdotAC);
            AdotAB = -AdotAB;
            AdotAC = -AdotAC;
            if (AdotAC <= F64.C0 && AdotAB <= F64.C0)
            {
                //It is A!
                State = SimplexState.Point;
                U = F64.C1;
                point = A;
                return;
            }

            //Check to see if it's outside B.
            //TODO: Note that in a boolean-style GJK, it shouldn't be possible to be outside B.
            Fixed64 BdotAB, BdotAC;
            FixedV3.Dot(ref ab, ref B, out BdotAB);
            FixedV3.Dot(ref ac, ref B, out BdotAC);
            BdotAB = -BdotAB;
            BdotAC = -BdotAC;
            if (BdotAB >= F64.C0 && BdotAC <= BdotAB)
            {
                //It is B!
                State = SimplexState.Point;
                A = B;
                U = F64.C1;
                SimplexA.A = SimplexA.B;
                SimplexB.A = SimplexB.B;
                point = B;
                return;
            }

            //Check to see if it's outside AB.
            Fixed64 vc = AdotAB * BdotAC - BdotAB * AdotAC;
            if (vc <= F64.C0 && AdotAB > F64.C0 && BdotAB < F64.C0)//Note > and < instead of => <=; avoids possibly division by zero
            {
                State = SimplexState.Segment;
                V = AdotAB / (AdotAB - BdotAB);
                U = F64.C1 - V;

                FixedV3.Multiply(ref ab, V, out point);
                FixedV3.Add(ref point, ref A, out point);
                return;
            }

            //Check to see if it's outside C.
            //TODO: Note that in a boolean-style GJK, it shouldn't be possible to be outside C.
            Fixed64 CdotAB, CdotAC;
            FixedV3.Dot(ref ab, ref C, out CdotAB);
            FixedV3.Dot(ref ac, ref C, out CdotAC);
            CdotAB = -CdotAB;
            CdotAC = -CdotAC;
            if (CdotAC >= F64.C0 && CdotAB <= CdotAC)
            {
                //It is C!
                State = SimplexState.Point;
                A = C;
                SimplexA.A = SimplexA.C;
                SimplexB.A = SimplexB.C;
                U = F64.C1;
                point = A;
                return;
            }

            //Check if it's outside AC.            
            //Fix64 AdotAB, AdotAC;
            //Vector3.Dot(ref ab, ref A, out AdotAB);
            //Vector3.Dot(ref ac, ref A, out AdotAC);
            //AdotAB = -AdotAB;
            //AdotAC = -AdotAC;
            Fixed64 vb = CdotAB * AdotAC - AdotAB * CdotAC;
            if (vb <= F64.C0 && AdotAC > F64.C0 && CdotAC < F64.C0)//Note > instead of >= and < instead of <=; prevents bad denominator
            {
                //Get rid of B.  Compress C into B.
                State = SimplexState.Segment;
                B = C;
                SimplexA.B = SimplexA.C;
                SimplexB.B = SimplexB.C;
                V = AdotAC / (AdotAC - CdotAC);
                U = F64.C1 - V;
                FixedV3.Multiply(ref ac, V, out point);
                FixedV3.Add(ref point, ref A, out point);
                return;
            }

            //Check if it's outside BC.
            //Fix64 BdotAB, BdotAC;
            //Vector3.Dot(ref ab, ref B, out BdotAB);
            //Vector3.Dot(ref ac, ref B, out BdotAC);
            //BdotAB = -BdotAB;
            //BdotAC = -BdotAC;
            Fixed64 va = BdotAB * CdotAC - CdotAB * BdotAC;
            Fixed64 d3d4;
            Fixed64 d6d5;
            if (va <= F64.C0 && (d3d4 = BdotAC - BdotAB) > F64.C0 && (d6d5 = CdotAB - CdotAC) > F64.C0)//Note > instead of >= and < instead of <=; prevents bad denominator
            {
                //Throw away A.  C->A.
                //TODO: Does B->A, C->B work better?
                State = SimplexState.Segment;
                A = C;
                SimplexA.A = SimplexA.C;
                SimplexB.A = SimplexB.C;
                U = d3d4 / (d3d4 + d6d5);
                V = F64.C1 - U;

                FixedV3 bc;
                FixedV3.Subtract(ref C, ref B, out bc);
                FixedV3.Multiply(ref bc, U, out point);
                FixedV3.Add(ref point, ref B, out point);
                return;
            }


            //On the face of the triangle.
            Fixed64 denom = F64.C1 / (va + vb + vc);
            V = vb * denom;
            W = vc * denom;
            U = F64.C1 - V - W;
            FixedV3.Multiply(ref ab, V, out point);
            FixedV3 acw;
            FixedV3.Multiply(ref ac, W, out acw);
            FixedV3.Add(ref A, ref point, out point);
            FixedV3.Add(ref point, ref acw, out point);




        }

        ///<summary>
        /// Gets the point on the tetrahedron closest to the origin.
        ///</summary>
        ///<param name="point">Closest point to the origin.</param>
        ///<returns>Whether or not the tetrahedron encloses the origin.</returns>
        public bool GetPointOnTetrahedronClosestToOrigin(out FixedV3 point)
        {

            //Thanks to the fact that D is new and that we know that the origin is within the extruded
            //triangular prism of ABC (and on the "D" side of ABC),
            //we can immediately ignore voronoi regions:
            //A, B, C, AC, AB, BC, ABC
            //and only consider:
            //D, DA, DB, DC, DAC, DCB, DBA

            //There is some overlap of calculations in this method, since DAC, DCB, and DBA are tested fully.


            PairSimplex minimumSimplex = new PairSimplex();
            point = new FixedV3();
            Fixed64 minimumDistance = Fixed64.MaxValue;


            PairSimplex candidate;
            Fixed64 candidateDistance;
            FixedV3 candidatePoint;
            if (TryTetrahedronTriangle(ref A, ref C, ref D,
                                       ref SimplexA.A, ref SimplexA.C, ref SimplexA.D,
                                       ref SimplexB.A, ref SimplexB.C, ref SimplexB.D,
                                       errorTolerance,
                                       ref B, out candidate, out candidatePoint))
            {
                point = candidatePoint;
                minimumSimplex = candidate;
                minimumDistance = candidatePoint.LengthSquared();
            }

            //Try BDC instead of CBD
            if (TryTetrahedronTriangle(ref B, ref D, ref C,
                                       ref SimplexA.B, ref SimplexA.D, ref SimplexA.C,
                                       ref SimplexB.B, ref SimplexB.D, ref SimplexB.C,
                                       errorTolerance,
                                       ref A, out candidate, out candidatePoint) &&
                (candidateDistance = candidatePoint.LengthSquared()) < minimumDistance)
            {
                point = candidatePoint;
                minimumSimplex = candidate;
                minimumDistance = candidateDistance;
            }

            //Try ADB instead of BAD
            if (TryTetrahedronTriangle(ref A, ref D, ref B,
                                       ref SimplexA.A, ref SimplexA.D, ref SimplexA.B,
                                       ref SimplexB.A, ref SimplexB.D, ref SimplexB.B,
                                       errorTolerance,
                                       ref C, out candidate, out candidatePoint) &&
                (candidateDistance = candidatePoint.LengthSquared()) < minimumDistance)
            {
                point = candidatePoint;
                minimumSimplex = candidate;
                minimumDistance = candidateDistance;
            }

            if (TryTetrahedronTriangle(ref A, ref B, ref C,
                                       ref SimplexA.A, ref SimplexA.B, ref SimplexA.C,
                                       ref SimplexB.A, ref SimplexB.B, ref SimplexB.C,
                                       errorTolerance,
                                       ref D, out candidate, out candidatePoint) &&
                (candidateDistance = candidatePoint.LengthSquared()) < minimumDistance)
            {
                point = candidatePoint;
                minimumSimplex = candidate;
                minimumDistance = candidateDistance;
            }


            if (minimumDistance < Fixed64.MaxValue)
            {
                minimumSimplex.LocalTransformB = LocalTransformB;
                minimumSimplex.previousDistanceToClosest = previousDistanceToClosest;
                minimumSimplex.errorTolerance = errorTolerance;
                this = minimumSimplex;
                return false;
            }
            return true;
        }


        private static bool TryTetrahedronTriangle(ref FixedV3 A, ref FixedV3 B, ref FixedV3 C,
                                                   ref FixedV3 A1, ref FixedV3 B1, ref FixedV3 C1,
                                                   ref FixedV3 A2, ref FixedV3 B2, ref FixedV3 C2,
                                                   Fixed64 errorTolerance,
                                                   ref FixedV3 otherPoint, out PairSimplex simplex, out FixedV3 point)
        {
            //Note that there may be some extra terms that can be removed from this process.
            //Some conditions could use less parameters, since it is known that the origin
            //is not 'behind' BC or AC.

            simplex = new PairSimplex();
            point = new FixedV3();


            FixedV3 ab, ac;
            FixedV3.Subtract(ref B, ref A, out ab);
            FixedV3.Subtract(ref C, ref A, out ac);
            FixedV3 normal;
            FixedV3.Cross(ref ab, ref ac, out normal);
            Fixed64 AdotN, ADdotN;
            FixedV3 AD;
            FixedV3.Subtract(ref otherPoint, ref A, out AD);
            FixedV3.Dot(ref A, ref normal, out AdotN);
            FixedV3.Dot(ref AD, ref normal, out ADdotN);

            //If (-A * N) * (AD * N) < 0, D and O are on opposite sides of the triangle.
            if (AdotN * ADdotN >= -Toolbox.Epsilon * errorTolerance)
            {
                //The point we are comparing against the triangle is 0,0,0, so instead of storing an "A->P" vector,
                //just use -A.
                //Same for B->, C->P...

                //Check to see if it's outside A.
                //TODO: Note that in a boolean-style GJK, it shouldn't be possible to be outside A.
                Fixed64 AdotAB, AdotAC;
                FixedV3.Dot(ref ab, ref A, out AdotAB);
                FixedV3.Dot(ref ac, ref A, out AdotAC);
                AdotAB = -AdotAB;
                AdotAC = -AdotAC;
                if (AdotAC <= F64.C0 && AdotAB <= F64.C0)
                {
                    //It is A!
                    simplex.State = SimplexState.Point;
                    simplex.A = A;
                    simplex.U = F64.C1;
                    simplex.SimplexA.A = A1;
                    simplex.SimplexB.A = A2;
                    point = A;
                    return true;
                }

                //Check to see if it's outside B.
                //TODO: Note that in a boolean-style GJK, it shouldn't be possible to be outside B.
                Fixed64 BdotAB, BdotAC;
                FixedV3.Dot(ref ab, ref B, out BdotAB);
                FixedV3.Dot(ref ac, ref B, out BdotAC);
                BdotAB = -BdotAB;
                BdotAC = -BdotAC;
                if (BdotAB >= F64.C0 && BdotAC <= BdotAB)
                {
                    //It is B!
                    simplex.State = SimplexState.Point;
                    simplex.A = B;
                    simplex.U = F64.C1;
                    simplex.SimplexA.A = B1;
                    simplex.SimplexB.A = B2;
                    point = B;
                    return true;
                }

                //Check to see if it's outside AB.
                Fixed64 vc = AdotAB * BdotAC - BdotAB * AdotAC;
                if (vc <= F64.C0 && AdotAB > F64.C0 && BdotAB < F64.C0) //Note > and < instead of => <=; avoids possibly division by zero
                {
                    simplex.State = SimplexState.Segment;
                    simplex.V = AdotAB / (AdotAB - BdotAB);
                    simplex.U = F64.C1 - simplex.V;
                    simplex.A = A;
                    simplex.B = B;
                    simplex.SimplexA.A = A1;
                    simplex.SimplexB.A = A2;
                    simplex.SimplexA.B = B1;
                    simplex.SimplexB.B = B2;

                    FixedV3.Multiply(ref ab, simplex.V, out point);
                    FixedV3.Add(ref point, ref A, out point);
                    return true;
                }

                //Check to see if it's outside C.
                //TODO: Note that in a boolean-style GJK, it shouldn't be possible to be outside C.
                Fixed64 CdotAB, CdotAC;
                FixedV3.Dot(ref ab, ref C, out CdotAB);
                FixedV3.Dot(ref ac, ref C, out CdotAC);
                CdotAB = -CdotAB;
                CdotAC = -CdotAC;
                if (CdotAC >= F64.C0 && CdotAB <= CdotAC)
                {
                    //It is C!
                    simplex.State = SimplexState.Point;
                    simplex.A = C;
                    simplex.U = F64.C1;
                    simplex.SimplexA.A = C1;
                    simplex.SimplexB.A = C2;
                    point = C;
                    return true;
                }

                //Check if it's outside AC.            
                //Fix64 AdotAB, AdotAC;
                //Vector3.Dot(ref ab, ref A, out AdotAB);
                //Vector3.Dot(ref ac, ref A, out AdotAC);
                //AdotAB = -AdotAB;
                //AdotAC = -AdotAC;
                Fixed64 vb = CdotAB * AdotAC - AdotAB * CdotAC;
                if (vb <= F64.C0 && AdotAC > F64.C0 && CdotAC < F64.C0) //Note > instead of >= and < instead of <=; prevents bad denominator
                {
                    simplex.State = SimplexState.Segment;
                    simplex.A = A;
                    simplex.B = C;
                    simplex.SimplexA.A = A1;
                    simplex.SimplexA.B = C1;
                    simplex.SimplexB.A = A2;
                    simplex.SimplexB.B = C2;
                    simplex.V = AdotAC / (AdotAC - CdotAC);
                    simplex.U = F64.C1 - simplex.V;
                    FixedV3.Multiply(ref ac, simplex.V, out point);
                    FixedV3.Add(ref point, ref A, out point);
                    return true;
                }

                //Check if it's outside BC.
                //Fix64 BdotAB, BdotAC;
                //Vector3.Dot(ref ab, ref B, out BdotAB);
                //Vector3.Dot(ref ac, ref B, out BdotAC);
                //BdotAB = -BdotAB;
                //BdotAC = -BdotAC;
                Fixed64 va = BdotAB * CdotAC - CdotAB * BdotAC;
                Fixed64 d3d4;
                Fixed64 d6d5;
                if (va <= F64.C0 && (d3d4 = BdotAC - BdotAB) > F64.C0 && (d6d5 = CdotAB - CdotAC) > F64.C0)//Note > instead of >= and < instead of <=; prevents bad denominator
                {
                    simplex.State = SimplexState.Segment;
                    simplex.A = B;
                    simplex.B = C;
                    simplex.SimplexA.A = B1;
                    simplex.SimplexA.B = C1;
                    simplex.SimplexB.A = B2;
                    simplex.SimplexB.B = C2;
                    simplex.V = d3d4 / (d3d4 + d6d5);
                    simplex.U = F64.C1 - simplex.V;

                    FixedV3 bc;
                    FixedV3.Subtract(ref C, ref B, out bc);
                    FixedV3.Multiply(ref bc, simplex.V, out point);
                    FixedV3.Add(ref point, ref B, out point);
                    return true;
                }


                //On the face of the triangle.
                simplex.A = A;
                simplex.B = B;
                simplex.C = C;
                simplex.SimplexA.A = A1;
                simplex.SimplexA.B = B1;
                simplex.SimplexA.C = C1;
                simplex.SimplexB.A = A2;
                simplex.SimplexB.B = B2;
                simplex.SimplexB.C = C2;
                simplex.State = SimplexState.Triangle;
                Fixed64 denom = F64.C1 / (va + vb + vc);
                simplex.W = vc * denom;
                simplex.V = vb * denom;
                simplex.U = F64.C1 - simplex.V - simplex.W;
                FixedV3.Multiply(ref ab, simplex.V, out point);
                FixedV3 acw;
                FixedV3.Multiply(ref ac, simplex.W, out acw);
                FixedV3.Add(ref A, ref point, out point);
                FixedV3.Add(ref point, ref acw, out point);
                return true;
            }
            return false;
        }


        internal Fixed64 errorTolerance;
        ///<summary>
        /// Gets the error tolerance of the simplex.
        ///</summary>
        public Fixed64 ErrorTolerance
        {
            get
            {
                return errorTolerance;
            }
        }
        Fixed64 previousDistanceToClosest;
        ///<summary>
        /// Adds a new point to the simplex.
        ///</summary>
        ///<param name="shapeA">First shape in the pair.</param>
        ///<param name="shapeB">Second shape in the pair.</param>
        ///<param name="iterationCount">Current iteration count.</param>
        ///<param name="closestPoint">Current point on simplex closest to origin.</param>
        ///<returns>Whether or not GJK should exit due to a lack of progression.</returns>
        public bool GetNewSimplexPoint(ConvexShape shapeA, ConvexShape shapeB, int iterationCount, ref FixedV3 closestPoint)
        {
            FixedV3 negativeDirection;
            FixedV3.Negate(ref closestPoint, out negativeDirection);
            FixedV3 sa, sb;
            shapeA.GetLocalExtremePointWithoutMargin(ref negativeDirection, out sa);
            shapeB.GetExtremePointWithoutMargin(closestPoint, ref LocalTransformB, out sb);
            FixedV3 S;
            FixedV3.Subtract(ref sa, ref sb, out S);
            //If S is not further towards the origin along negativeDirection than closestPoint, then we're done.
            Fixed64 dotS;
            FixedV3.Dot(ref S, ref negativeDirection, out dotS); //-P * S
            Fixed64 distanceToClosest = closestPoint.LengthSquared();

            Fixed64 progression = dotS + distanceToClosest;
            //It's likely that the system is oscillating between two or more states, usually because of a degenerate simplex.
            //Rather than detect specific problem cases, this approach just lets it run and catches whatever falls through.
            //During oscillation, one of the states is usually just BARELY outside of the numerical tolerance.
            //After a bunch of iterations, the system lets it pick the 'better' one.
            if (iterationCount > GJKToolbox.HighGJKIterations && distanceToClosest - previousDistanceToClosest < DistanceConvergenceEpsilon * errorTolerance)
                return true;
            if (distanceToClosest < previousDistanceToClosest)
                previousDistanceToClosest = distanceToClosest;

            //If "A" is the new point always, then the switch statement can be removed
            //in favor of just pushing three points up.
            switch (State)
            {
                case SimplexState.Point:
                    if (progression <= (errorTolerance = MathHelper.Max(A.LengthSquared(), S.LengthSquared())) * ProgressionEpsilon)
                        return true;

                    State = SimplexState.Segment;
                    B = S;
                    SimplexA.B = sa;
                    SimplexB.B = sb;
                    return false;
                case SimplexState.Segment:
                    if (progression <= (errorTolerance = MathHelper.Max(MathHelper.Max(A.LengthSquared(), B.LengthSquared()), S.LengthSquared())) * ProgressionEpsilon)
                        return true;

                    State = SimplexState.Triangle;
                    C = S;
                    SimplexA.C = sa;
                    SimplexB.C = sb;
                    return false;
                case SimplexState.Triangle:
                    if (progression <= (errorTolerance = MathHelper.Max(MathHelper.Max(A.LengthSquared(), B.LengthSquared()), MathHelper.Max(C.LengthSquared(), S.LengthSquared()))) * ProgressionEpsilon)
                        return true;

                    State = SimplexState.Tetrahedron;
                    D = S;
                    SimplexA.D = sa;
                    SimplexB.D = sb;
                    return false;
            }
            return false;
        }

        ///<summary>
        /// Gets the closest points by using the barycentric coordinates and shape simplex contributions.
        ///</summary>
        ///<param name="closestPointA">Closest point on shape A.</param>
        ///<param name="closestPointB">Closest point on shape B.</param>
        public void GetClosestPoints(out FixedV3 closestPointA, out FixedV3 closestPointB)
        {
            //A * U + B * V + C * W
            switch (State)
            {
                case SimplexState.Point:
                    closestPointA = SimplexA.A;
                    closestPointB = SimplexB.A;
                    return;
                case SimplexState.Segment:
                    FixedV3 temp;
                    FixedV3.Multiply(ref SimplexA.A, U, out closestPointA);
                    FixedV3.Multiply(ref SimplexA.B, V, out temp);
                    FixedV3.Add(ref closestPointA, ref temp, out closestPointA);

                    FixedV3.Multiply(ref SimplexB.A, U, out closestPointB);
                    FixedV3.Multiply(ref SimplexB.B, V, out temp);
                    FixedV3.Add(ref closestPointB, ref temp, out closestPointB);
                    return;
                case SimplexState.Triangle:
                    FixedV3.Multiply(ref SimplexA.A, U, out closestPointA);
                    FixedV3.Multiply(ref SimplexA.B, V, out temp);
                    FixedV3.Add(ref closestPointA, ref temp, out closestPointA);
                    FixedV3.Multiply(ref SimplexA.C, W, out temp);
                    FixedV3.Add(ref closestPointA, ref temp, out closestPointA);

                    FixedV3.Multiply(ref SimplexB.A, U, out closestPointB);
                    FixedV3.Multiply(ref SimplexB.B, V, out temp);
                    FixedV3.Add(ref closestPointB, ref temp, out closestPointB);
                    FixedV3.Multiply(ref SimplexB.C, W, out temp);
                    FixedV3.Add(ref closestPointB, ref temp, out closestPointB);
                    return;
            }
            closestPointA = Toolbox.ZeroVector;
            closestPointB = Toolbox.ZeroVector;

        }

        internal void VerifyContributions()
        {
            switch (State)
            {
                case SimplexState.Point:
                    if (FixedV3.Distance(SimplexA.A - SimplexB.A, A) > F64.C1em4)
                        Debug.WriteLine("break.");
                    break;
                case SimplexState.Segment:
                    if (FixedV3.Distance(SimplexA.A - SimplexB.A, A) > F64.C1em4)
                        Debug.WriteLine("break.");

                    if (FixedV3.Distance(SimplexA.B - SimplexB.B, B) > F64.C1em4)
                        Debug.WriteLine("break.");
                    break;
                case SimplexState.Triangle:
                    if (FixedV3.Distance(SimplexA.A - SimplexB.A, A) > F64.C1em4)
                        Debug.WriteLine("break.");

                    if (FixedV3.Distance(SimplexA.B - SimplexB.B, B) > F64.C1em4)
                        Debug.WriteLine("break.");

                    if (FixedV3.Distance(SimplexA.C - SimplexB.C, C) > F64.C1em4)
                        Debug.WriteLine("break.");
                    break;

                case SimplexState.Tetrahedron:
                    if (FixedV3.Distance(SimplexA.A - SimplexB.A, A) > F64.C1em4)
                        Debug.WriteLine("break.");

                    if (FixedV3.Distance(SimplexA.B - SimplexB.B, B) > F64.C1em4)
                        Debug.WriteLine("break.");

                    if (FixedV3.Distance(SimplexA.C - SimplexB.C, C) > F64.C1em4)
                        Debug.WriteLine("break.");

                    if (FixedV3.Distance(SimplexA.D - SimplexB.D, D) > F64.C1em4)
                        Debug.WriteLine("break.");
                    break;
            }
        }
    }

}

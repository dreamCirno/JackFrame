using System;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.CollisionTests.CollisionAlgorithms
{
    ///<summary>
    /// Helper class that supports other systems using minkowski space operations.
    ///</summary>
    public static class MinkowskiToolbox
    {
        ///<summary>
        /// Gets the local transform of B in the space of A.
        ///</summary>
        ///<param name="transformA">First transform.</param>
        ///<param name="transformB">Second transform.</param>
        ///<param name="localTransformB">Transform of B in the local space of A.</param>
        public static void GetLocalTransform(ref RigidTransform transformA, ref RigidTransform transformB,
                                             out RigidTransform localTransformB)
        {
            //Put B into A's space.
            FixedQuaternion conjugateOrientationA;
            FixedQuaternion.Conjugate(ref transformA.Orientation, out conjugateOrientationA);
            FixedQuaternion.Concatenate(ref transformB.Orientation, ref conjugateOrientationA, out localTransformB.Orientation);
            FixedV3.Subtract(ref transformB.Position, ref transformA.Position, out localTransformB.Position);
            FixedQuaternion.Transform(ref localTransformB.Position, ref conjugateOrientationA, out localTransformB.Position);
        }

        ///<summary>
        /// Gets the extreme point of the minkowski difference of shapeA and shapeB in the local space of shapeA.
        ///</summary>
        ///<param name="shapeA">First shape.</param>
        ///<param name="shapeB">Second shape.</param>
        ///<param name="direction">Extreme point direction in local space.</param>
        ///<param name="localTransformB">Transform of shapeB in the local space of A.</param>
        ///<param name="extremePoint">The extreme point in the local space of A.</param>
        public static void GetLocalMinkowskiExtremePoint(ConvexShape shapeA, ConvexShape shapeB, ref FixedV3 direction, ref RigidTransform localTransformB, out FixedV3 extremePoint)
        {
            //Extreme point of A-B along D = (extreme point of A along D) - (extreme point of B along -D)
            shapeA.GetLocalExtremePointWithoutMargin(ref direction, out extremePoint);
            FixedV3 v;
            FixedV3 negativeN;
            FixedV3.Negate(ref direction, out negativeN);
            shapeB.GetExtremePointWithoutMargin(negativeN, ref localTransformB, out v);
            FixedV3.Subtract(ref extremePoint, ref v, out extremePoint);

            ExpandMinkowskiSum(shapeA.collisionMargin, shapeB.collisionMargin, ref direction, out v);
            FixedV3.Add(ref extremePoint, ref v, out extremePoint);


        }

        ///<summary>
        /// Gets the extreme point of the minkowski difference of shapeA and shapeB in the local space of shapeA.
        ///</summary>
        ///<param name="shapeA">First shape.</param>
        ///<param name="shapeB">Second shape.</param>
        ///<param name="direction">Extreme point direction in local space.</param>
        ///<param name="localTransformB">Transform of shapeB in the local space of A.</param>
        /// <param name="extremePointA">The extreme point on shapeA.</param>
        ///<param name="extremePoint">The extreme point in the local space of A.</param>
        public static void GetLocalMinkowskiExtremePoint(ConvexShape shapeA, ConvexShape shapeB, ref FixedV3 direction, ref RigidTransform localTransformB,
                                                 out FixedV3 extremePointA, out FixedV3 extremePoint)
        {
            //Extreme point of A-B along D = (extreme point of A along D) - (extreme point of B along -D)
            shapeA.GetLocalExtremePointWithoutMargin(ref direction, out extremePointA);
            FixedV3 v;
            FixedV3.Negate(ref direction, out v);
            FixedV3 extremePointB;
            shapeB.GetExtremePointWithoutMargin(v, ref localTransformB, out extremePointB);

            ExpandMinkowskiSum(shapeA.collisionMargin, shapeB.collisionMargin, direction, ref extremePointA, ref extremePointB);
            FixedV3.Subtract(ref extremePointA, ref extremePointB, out extremePoint);


        }

        ///<summary>
        /// Gets the extreme point of the minkowski difference of shapeA and shapeB in the local space of shapeA.
        ///</summary>
        ///<param name="shapeA">First shape.</param>
        ///<param name="shapeB">Second shape.</param>
        ///<param name="direction">Extreme point direction in local space.</param>
        ///<param name="localTransformB">Transform of shapeB in the local space of A.</param>
        /// <param name="extremePointA">The extreme point on shapeA.</param>
        /// <param name="extremePointB">The extreme point on shapeB.</param>
        ///<param name="extremePoint">The extreme point in the local space of A.</param>
        public static void GetLocalMinkowskiExtremePoint(ConvexShape shapeA, ConvexShape shapeB, ref FixedV3 direction, ref RigidTransform localTransformB,
                                                 out FixedV3 extremePointA, out FixedV3 extremePointB, out FixedV3 extremePoint)
        {
            //Extreme point of A-B along D = (extreme point of A along D) - (extreme point of B along -D)
            shapeA.GetLocalExtremePointWithoutMargin(ref direction, out extremePointA);
            FixedV3 v;
            FixedV3.Negate(ref direction, out v);
            shapeB.GetExtremePointWithoutMargin(v, ref localTransformB, out extremePointB);

            ExpandMinkowskiSum(shapeA.collisionMargin, shapeB.collisionMargin, direction, ref extremePointA, ref extremePointB);
            FixedV3.Subtract(ref extremePointA, ref extremePointB, out extremePoint);


        }

        ///<summary>
        /// Gets the extreme point of the minkowski difference of shapeA and shapeB in the local space of shapeA, without a margin.
        ///</summary>
        ///<param name="shapeA">First shape.</param>
        ///<param name="shapeB">Second shape.</param>
        ///<param name="direction">Extreme point direction in local space.</param>
        ///<param name="localTransformB">Transform of shapeB in the local space of A.</param>
        ///<param name="extremePoint">The extreme point in the local space of A.</param>
        public static void GetLocalMinkowskiExtremePointWithoutMargin(ConvexShape shapeA, ConvexShape shapeB, ref FixedV3 direction, ref RigidTransform localTransformB, out FixedV3 extremePoint)
        {
            //Extreme point of A-B along D = (extreme point of A along D) - (extreme point of B along -D)
            shapeA.GetLocalExtremePointWithoutMargin(ref direction, out extremePoint);
            FixedV3 extremePointB;
            FixedV3 negativeN;
            FixedV3.Negate(ref direction, out negativeN);
            shapeB.GetExtremePointWithoutMargin(negativeN, ref localTransformB, out extremePointB);
            FixedV3.Subtract(ref extremePoint, ref extremePointB, out extremePoint);

        }



        ///<summary>
        /// Computes the expansion of the minkowski sum due to margins in a given direction.
        ///</summary>
        ///<param name="marginA">First margin.</param>
        ///<param name="marginB">Second margin.</param>
        ///<param name="direction">Extreme point direction.</param>
        ///<param name="contribution">Margin contribution to the extreme point.</param>
        public static void ExpandMinkowskiSum(Fixed64 marginA, Fixed64 marginB, ref FixedV3 direction, out FixedV3 contribution)
        {
            Fixed64 lengthSquared = direction.LengthSquared();
            if (lengthSquared > Toolbox.Epsilon)
            {
                //The contribution to the minkowski sum by the margin is:
                //direction * marginA - (-direction) * marginB.
                FixedV3.Multiply(ref direction, (marginA + marginB) / Fixed64.Sqrt(lengthSquared), out contribution);

            }
            else
            {
                contribution = new FixedV3();
            }


        }


        ///<summary>
        /// Computes the expansion of the minkowski sum due to margins in a given direction.
        ///</summary>
        ///<param name="marginA">First margin.</param>
        ///<param name="marginB">Second margin.</param>
        ///<param name="direction">Extreme point direction.</param>
        ///<param name="toExpandA">Margin contribution to the shapeA.</param>
        ///<param name="toExpandB">Margin contribution to the shapeB.</param>
        public static void ExpandMinkowskiSum(Fixed64 marginA, Fixed64 marginB, FixedV3 direction, ref FixedV3 toExpandA, ref FixedV3 toExpandB)
        {
            Fixed64 lengthSquared = direction.LengthSquared();
            if (lengthSquared > Toolbox.Epsilon)
            {
                lengthSquared = F64.C1 / Fixed64.Sqrt(lengthSquared);   
                //The contribution to the minkowski sum by the margin is:
                //direction * marginA - (-direction) * marginB. 
                FixedV3 contribution;
                FixedV3.Multiply(ref direction, marginA * lengthSquared, out contribution);
                FixedV3.Add(ref toExpandA, ref contribution, out toExpandA);
                FixedV3.Multiply(ref direction, marginB * lengthSquared, out contribution);
                FixedV3.Subtract(ref toExpandB, ref contribution, out toExpandB);
            }
            //If the direction is too small, then the expansion values are left unchanged.

        }
    }
}

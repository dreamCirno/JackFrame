﻿using FixMath.NET;

namespace BEPUPhysics1int.Constraints.TwoEntity.JointLimits
{
    /// <summary>
    /// Constraint which prevents the connected entities from rotating relative to each other around an axis beyond given limits.
    /// </summary>
    public class RevoluteLimit : JointLimit, I2DImpulseConstraintWithError, I2DJacobianConstraint
    {
        private readonly JointBasis2D basis = new JointBasis2D();

        private FixedV2 accumulatedImpulse;
        private FixedV2 biasVelocity;
        private FixedV3 jacobianMaxA;
        private FixedV3 jacobianMaxB;
        private FixedV3 jacobianMinA;
        private FixedV3 jacobianMinB;
        private bool maxIsActive;
        private bool minIsActive;
        private FixedV2 error;
        private FixedV3 localTestAxis;

        /// <summary>
        /// Naximum angle that entities can twist.
        /// </summary>
        protected Fixed64 maximumAngle;

        /// <summary>
        /// Minimum angle that entities can twist.
        /// </summary>
        protected Fixed64 minimumAngle;

        private FixedV3 worldTestAxis;
        private FixedV2 velocityToImpulse;

        /// <summary>
        /// Constructs a new constraint which prevents the connected entities from rotating relative to each other around an axis beyond given limits.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the TestAxis (or its entity-local version) and the Basis.
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public RevoluteLimit()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a new constraint which prevents the connected entities from rotating relative to each other around an axis beyond given limits.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        /// <param name="limitedAxis">Axis of rotation to be limited.</param>
        /// <param name="testAxis">Axis attached to connectionB that is tested to determine the current angle.
        /// Will also be used as the base rotation axis representing 0 degrees.</param>
        /// <param name="minimumAngle">Minimum twist angle allowed.</param>
        /// <param name="maximumAngle">Maximum twist angle allowed.</param>
        public RevoluteLimit(Entity connectionA, Entity connectionB, FixedV3 limitedAxis, FixedV3 testAxis, Fixed64 minimumAngle, Fixed64 maximumAngle)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;

            //Put the axes into the joint transform of A.
            basis.rotationMatrix = this.connectionA.orientationMatrix;
            basis.SetWorldAxes(limitedAxis, testAxis);

            //Put the axes into the 'joint transform' of B too.
            TestAxis = basis.xAxis;

            MinimumAngle = minimumAngle;
            MaximumAngle = maximumAngle;
        }


        /// <summary>
        /// Constructs a new constraint which prevents the connected entities from rotating relative to each other around an axis beyond given limits.
        /// Using this constructor will leave the limit uninitialized.  Before using the limit in a simulation, be sure to set the basis axes using
        /// Basis.SetLocalAxes or Basis.SetWorldAxes and the test axis using the LocalTestAxis or TestAxis properties.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        public RevoluteLimit(Entity connectionA, Entity connectionB)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;
        }

        /// <summary>
        /// Gets the basis attached to entity A.
        /// The primary axis represents the limited axis of rotation.  The 'measurement plane' which the test axis is tested against is based on this primary axis.
        /// The x axis defines the 'base' direction on the measurement plane corresponding to 0 degrees of relative rotation.
        /// </summary>
        public JointBasis2D Basis
        {
            get { return basis; }
        }

        /// <summary>
        /// Gets or sets the axis attached to entity B in its local space that will be tested against the limits.
        /// </summary>
        public FixedV3 LocalTestAxis
        {
            get { return localTestAxis; }
            set
            {
                localTestAxis = FixedV3.Normalize(value);
                BEPUMatrix3x3.Transform(ref localTestAxis, ref connectionB.orientationMatrix, out worldTestAxis);
            }
        }

        /// <summary>
        /// Gets or sets the maximum angle that entities can twist.
        /// </summary>
        public Fixed64 MaximumAngle
        {
            get { return maximumAngle; }
            set
            {
                maximumAngle = value % (MathHelper.TwoPi);
                if (minimumAngle > MathHelper.Pi)
                    minimumAngle -= MathHelper.TwoPi;
                if (minimumAngle <= -MathHelper.Pi)
                    minimumAngle += MathHelper.TwoPi;
            }
        }

        /// <summary>
        /// Gets or sets the minimum angle that entities can twist.
        /// </summary>
        public Fixed64 MinimumAngle
        {
            get { return minimumAngle; }
            set
            {
                minimumAngle = value % (MathHelper.TwoPi);
                if (minimumAngle > MathHelper.Pi)
                    minimumAngle -= MathHelper.TwoPi;
                if (minimumAngle <= -MathHelper.Pi)
                    minimumAngle += MathHelper.TwoPi;
            }
        }

        /// <summary>
        /// Gets or sets the axis attached to entity B in world space that will be tested against the limits.
        /// </summary>
        public FixedV3 TestAxis
        {
            get { return worldTestAxis; }
            set
            {
                worldTestAxis = FixedV3.Normalize(value);
                BEPUMatrix3x3.TransformTranspose(ref worldTestAxis, ref connectionB.orientationMatrix, out localTestAxis);
            }
        }

        #region I2DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// The revolute limit is special; internally, it is sometimes two constraints.
        /// The X value of the vector is the "minimum" plane of the limit, and the Y value is the "maximum" plane.
        /// If a plane isn't active, its error is zero.
        /// </summary>
        public FixedV2 RelativeVelocity
        {
            get
            {
                if (isLimitActive)
                {
                    Fixed64 velocityA, velocityB;
                    FixedV2 toReturn = FixedV2.Zero;
                    if (minIsActive)
                    {
                        FixedV3.Dot(ref connectionA.angularVelocity, ref jacobianMinA, out velocityA);
                        FixedV3.Dot(ref connectionB.angularVelocity, ref jacobianMinB, out velocityB);
                        toReturn.X = velocityA + velocityB;
                    }
                    if (maxIsActive)
                    {
                        FixedV3.Dot(ref connectionA.angularVelocity, ref jacobianMaxA, out velocityA);
                        FixedV3.Dot(ref connectionB.angularVelocity, ref jacobianMaxB, out velocityB);
                        toReturn.Y = velocityA + velocityB;
                    }
                    return toReturn;
                }
                return new FixedV2();
            }
        }

        /// <summary>
        /// Gets the total impulse applied by this constraint.
        /// The x component corresponds to the minimum plane limit,
        /// while the y component corresponds to the maximum plane limit.
        /// </summary>
        public FixedV2 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the current constraint error.
        /// The x component corresponds to the minimum plane limit,
        /// while the y component corresponds to the maximum plane limit.
        /// </summary>
        public FixedV2 Error
        {
            get { return error; }
        }

        #endregion

        //Newer version of revolute limit will use up to two planes.  This is sort of like being a double-constraint, with two jacobians and everything.
        //Not going to solve both plane limits simultaneously because they can be redundant.  De-linking them will let the system deal with redundancy better.

        #region I2DJacobianConstraint Members

        /// <summary>
        /// Gets the linear jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobianX">First linear jacobian entry for the first connected entity.</param>
        /// <param name="jacobianY">Second linear jacobian entry for the first connected entity.</param>
        public void GetLinearJacobianA(out FixedV3 jacobianX, out FixedV3 jacobianY)
        {
            jacobianX = Toolbox.ZeroVector;
            jacobianY = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the linear jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobianX">First linear jacobian entry for the second connected entity.</param>
        /// <param name="jacobianY">Second linear jacobian entry for the second connected entity.</param>
        public void GetLinearJacobianB(out FixedV3 jacobianX, out FixedV3 jacobianY)
        {
            jacobianX = Toolbox.ZeroVector;
            jacobianY = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobianX">First angular jacobian entry for the first connected entity.</param>
        /// <param name="jacobianY">Second angular jacobian entry for the first connected entity.</param>
        public void GetAngularJacobianA(out FixedV3 jacobianX, out FixedV3 jacobianY)
        {
            jacobianX = jacobianMinA;
            jacobianY = jacobianMaxA;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobianX">First angular jacobian entry for the second connected entity.</param>
        /// <param name="jacobianY">Second angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out FixedV3 jacobianX, out FixedV3 jacobianY)
        {
            jacobianX = jacobianMinB;
            jacobianY = jacobianMaxB;
        }

        /// <summary>
        /// Gets the mass matrix of the revolute limit.
        /// The revolute limit is special; in terms of solving, it is
        /// actually sometimes TWO constraints; a minimum plane, and a
        /// maximum plane.  The M11 field represents the minimum plane mass matrix
        /// and the M22 field represents the maximum plane mass matrix.
        /// </summary>
        /// <param name="massMatrix">Mass matrix of the constraint.</param>
        public void GetMassMatrix(out BEPUMatrix2x2 massMatrix)
        {
            massMatrix.M11 = velocityToImpulse.X;
            massMatrix.M22 = velocityToImpulse.Y;
            massMatrix.M12 = F64.C0;
            massMatrix.M21 = F64.C0;
        }

        #endregion

        /// <summary>
        /// Computes one iteration of the constraint to meet the solver updateable's goal.
        /// </summary>
        /// <returns>The rough applied impulse magnitude.</returns>
        public override Fixed64 SolveIteration()
        {
            Fixed64 lambda;
            Fixed64 lambdaTotal = F64.C0;
            Fixed64 velocityA, velocityB;
            Fixed64 previousAccumulatedImpulse;
            if (minIsActive)
            {
                //Find the velocity contribution from each connection
                FixedV3.Dot(ref connectionA.angularVelocity, ref jacobianMinA, out velocityA);
                FixedV3.Dot(ref connectionB.angularVelocity, ref jacobianMinB, out velocityB);
                //Add in the constraint space bias velocity
                lambda = -(velocityA + velocityB) + biasVelocity.X - softness * accumulatedImpulse.X;

                //Transform to an impulse
                lambda *= velocityToImpulse.X;

                //Clamp accumulated impulse (can't go negative)
                previousAccumulatedImpulse = accumulatedImpulse.X;
                accumulatedImpulse.X = MathHelper.Max(accumulatedImpulse.X + lambda, F64.C0);
                lambda = accumulatedImpulse.X - previousAccumulatedImpulse;

                //Apply the impulse
                FixedV3 impulse;
                if (connectionA.isDynamic)
                {
                    FixedV3.Multiply(ref jacobianMinA, lambda, out impulse);
                    connectionA.ApplyAngularImpulse(ref impulse);
                }
                if (connectionB.isDynamic)
                {
                    FixedV3.Multiply(ref jacobianMinB, lambda, out impulse);
                    connectionB.ApplyAngularImpulse(ref impulse);
                }

                lambdaTotal += Fixed64.Abs(lambda);
            }
            if (maxIsActive)
            {
                //Find the velocity contribution from each connection
                FixedV3.Dot(ref connectionA.angularVelocity, ref jacobianMaxA, out velocityA);
                FixedV3.Dot(ref connectionB.angularVelocity, ref jacobianMaxB, out velocityB);
                //Add in the constraint space bias velocity
                lambda = -(velocityA + velocityB) + biasVelocity.Y - softness * accumulatedImpulse.Y;

                //Transform to an impulse
                lambda *= velocityToImpulse.Y;

                //Clamp accumulated impulse (can't go negative)
                previousAccumulatedImpulse = accumulatedImpulse.Y;
                accumulatedImpulse.Y = MathHelper.Max(accumulatedImpulse.Y + lambda, F64.C0);
                lambda = accumulatedImpulse.Y - previousAccumulatedImpulse;

                //Apply the impulse
                FixedV3 impulse;
                if (connectionA.isDynamic)
                {
                    FixedV3.Multiply(ref jacobianMaxA, lambda, out impulse);
                    connectionA.ApplyAngularImpulse(ref impulse);
                }
                if (connectionB.isDynamic)
                {
                    FixedV3.Multiply(ref jacobianMaxB, lambda, out impulse);
                    connectionB.ApplyAngularImpulse(ref impulse);
                }

                lambdaTotal += Fixed64.Abs(lambda);
            }
            return lambdaTotal;
        }

        ///<summary>
        /// Performs the frame's configuration step.
        ///</summary>
        ///<param name="dt">Timestep duration.</param>
        public override void Update(Fixed64 dt)
        {
            //Transform the axes into world space.
            basis.rotationMatrix = connectionA.orientationMatrix;
            basis.ComputeWorldSpaceAxes();
            BEPUMatrix3x3.Transform(ref localTestAxis, ref connectionB.orientationMatrix, out worldTestAxis);

            //Compute the plane normals.
            FixedV3 minPlaneNormal, maxPlaneNormal;
            //Rotate basisA y axis around the basisA primary axis.
            BEPUMatrix3x3 rotation;
            BEPUMatrix3x3.CreateFromAxisAngle(ref basis.primaryAxis, minimumAngle + MathHelper.PiOver2, out rotation);
            BEPUMatrix3x3.Transform(ref basis.xAxis, ref rotation, out minPlaneNormal);
            BEPUMatrix3x3.CreateFromAxisAngle(ref basis.primaryAxis, maximumAngle - MathHelper.PiOver2, out rotation);
            BEPUMatrix3x3.Transform(ref basis.xAxis, ref rotation, out maxPlaneNormal);

            //Compute the errors along the two normals.
            Fixed64 planePositionMin, planePositionMax;
            FixedV3.Dot(ref minPlaneNormal, ref worldTestAxis, out planePositionMin);
            FixedV3.Dot(ref maxPlaneNormal, ref worldTestAxis, out planePositionMax);


            Fixed64 span = GetDistanceFromMinimum(maximumAngle);

            //Early out and compute the determine the plane normal.
            if (span >= MathHelper.Pi)
            {
                if (planePositionMax > F64.C0 || planePositionMin > F64.C0)
                {
                    //It's in a perfectly valid configuration, so skip.
                    isActiveInSolver = false;
                    minIsActive = false;
                    maxIsActive = false;
                    error = FixedV2.Zero;
                    accumulatedImpulse = FixedV2.Zero;
                    isLimitActive = false;
                    return;
                }

                if (planePositionMax > planePositionMin)
                {
                    //It's quicker to escape out to the max plane than the min plane.
                    error.X = F64.C0;
                    error.Y = -planePositionMax;
                    accumulatedImpulse.X = F64.C0;
                    minIsActive = false;
                    maxIsActive = true;
                }
                else
                {
                    //It's quicker to escape out to the min plane than the max plane.
                    error.X = -planePositionMin;
                    error.Y = F64.C0;
                    accumulatedImpulse.Y = F64.C0;
                    minIsActive = true;
                    maxIsActive = false;
                }
                //There's never a non-degenerate situation where having both planes active with a span 
                //greater than pi is useful.
            }
            else
            {
                if (planePositionMax > F64.C0 && planePositionMin > F64.C0)
                {
                    //It's in a perfectly valid configuration, so skip.
                    isActiveInSolver = false;
                    minIsActive = false;
                    maxIsActive = false;
                    error = FixedV2.Zero;
                    accumulatedImpulse = FixedV2.Zero;
                    isLimitActive = false;
                    return;
                }

                if (planePositionMin <= F64.C0 && planePositionMax <= F64.C0)
                {
                    //Escape upward.
                    //Activate both planes.
                    error.X = -planePositionMin;
                    error.Y = -planePositionMax;
                    minIsActive = true;
                    maxIsActive = true;
                }
                else if (planePositionMin <= F64.C0)
                {
                    //It's quicker to escape out to the min plane than the max plane.
                    error.X = -planePositionMin;
                    error.Y = F64.C0;
                    accumulatedImpulse.Y = F64.C0;
                    minIsActive = true;
                    maxIsActive = false;
                }
                else
                {
                    //It's quicker to escape out to the max plane than the min plane.
                    error.X = F64.C0;
                    error.Y = -planePositionMax;
                    accumulatedImpulse.X = F64.C0;
                    minIsActive = false;
                    maxIsActive = true;
                }
            }
            isLimitActive = true;


            //****** VELOCITY BIAS ******//
            //Compute the correction velocity
            Fixed64 errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReduction, out softness);

            //Compute the jacobians
            if (minIsActive)
            {
                FixedV3.Cross(ref minPlaneNormal, ref worldTestAxis, out jacobianMinA);
                if (jacobianMinA.LengthSquared() < Toolbox.Epsilon)
                {
                    //The plane normal is aligned with the test axis.
                    //Use the basis's free axis.
                    jacobianMinA = basis.primaryAxis;
                }
                jacobianMinA.Normalize();
                jacobianMinB.X = -jacobianMinA.X;
                jacobianMinB.Y = -jacobianMinA.Y;
                jacobianMinB.Z = -jacobianMinA.Z;
            }
            if (maxIsActive)
            {
                FixedV3.Cross(ref maxPlaneNormal, ref worldTestAxis, out jacobianMaxA);
                if (jacobianMaxA.LengthSquared() < Toolbox.Epsilon)
                {
                    //The plane normal is aligned with the test axis.
                    //Use the basis's free axis.
                    jacobianMaxA = basis.primaryAxis;
                }
                jacobianMaxA.Normalize();
                jacobianMaxB.X = -jacobianMaxA.X;
                jacobianMaxB.Y = -jacobianMaxA.Y;
                jacobianMaxB.Z = -jacobianMaxA.Z;
            }

            //Error is always positive
            if (minIsActive)
            {
                biasVelocity.X = MathHelper.Min(MathHelper.Max(F64.C0, error.X - margin) * errorReduction, maxCorrectiveVelocity);
                if (bounciness > F64.C0)
                {
                    Fixed64 relativeVelocity;
                    Fixed64 dot;
                    //Find the velocity contribution from each connection
                    FixedV3.Dot(ref connectionA.angularVelocity, ref jacobianMinA, out relativeVelocity);
                    FixedV3.Dot(ref connectionB.angularVelocity, ref jacobianMinB, out dot);
                    relativeVelocity += dot;
                    biasVelocity.X = MathHelper.Max(biasVelocity.X, ComputeBounceVelocity(-relativeVelocity));
                }
            }
            if (maxIsActive)
            {
                biasVelocity.Y = MathHelper.Min(MathHelper.Max(F64.C0, error.Y - margin) * errorReduction, maxCorrectiveVelocity);
                if (bounciness > F64.C0)
                {
                    //Find the velocity contribution from each connection
                    if (maxIsActive)
                    {
                        Fixed64 relativeVelocity;
                        FixedV3.Dot(ref connectionA.angularVelocity, ref jacobianMaxA, out relativeVelocity);
                        Fixed64 dot;
                        FixedV3.Dot(ref connectionB.angularVelocity, ref jacobianMaxB, out dot);
                        relativeVelocity += dot;
                        biasVelocity.Y = MathHelper.Max(biasVelocity.Y, ComputeBounceVelocity(-relativeVelocity));
                    }
                }
            }


            //****** EFFECTIVE MASS MATRIX ******//
            //Connection A's contribution to the mass matrix
            Fixed64 minEntryA, minEntryB;
            Fixed64 maxEntryA, maxEntryB;
            FixedV3 transformedAxis;
            if (connectionA.isDynamic)
            {
                if (minIsActive)
                {
                    BEPUMatrix3x3.Transform(ref jacobianMinA, ref connectionA.inertiaTensorInverse, out transformedAxis);
                    FixedV3.Dot(ref transformedAxis, ref jacobianMinA, out minEntryA);
                }
                else
                    minEntryA = F64.C0;
                if (maxIsActive)
                {
                    BEPUMatrix3x3.Transform(ref jacobianMaxA, ref connectionA.inertiaTensorInverse, out transformedAxis);
                    FixedV3.Dot(ref transformedAxis, ref jacobianMaxA, out maxEntryA);
                }
                else
                    maxEntryA = F64.C0;
            }
            else
            {
                minEntryA = F64.C0;
                maxEntryA = F64.C0;
            }
            //Connection B's contribution to the mass matrix
            if (connectionB.isDynamic)
            {
                if (minIsActive)
                {
                    BEPUMatrix3x3.Transform(ref jacobianMinB, ref connectionB.inertiaTensorInverse, out transformedAxis);
                    FixedV3.Dot(ref transformedAxis, ref jacobianMinB, out minEntryB);
                }
                else
                    minEntryB = F64.C0;
                if (maxIsActive)
                {
                    BEPUMatrix3x3.Transform(ref jacobianMaxB, ref connectionB.inertiaTensorInverse, out transformedAxis);
                    FixedV3.Dot(ref transformedAxis, ref jacobianMaxB, out maxEntryB);
                }
                else
                    maxEntryB = F64.C0;
            }
            else
            {
                minEntryB = F64.C0;
                maxEntryB = F64.C0;
            }
            //Compute the inverse mass matrix
            //Notice that the mass matrix isn't linked, it's two separate ones.
            velocityToImpulse.X = F64.C1 / (softness + minEntryA + minEntryB);
            velocityToImpulse.Y = F64.C1 / (softness + maxEntryA + maxEntryB);


        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //****** WARM STARTING ******//
            //Apply accumulated impulse
            if (connectionA.isDynamic)
            {
                var impulse = new FixedV3();
                if (minIsActive)
                {
                    FixedV3.Multiply(ref jacobianMinA, accumulatedImpulse.X, out impulse);
                }
                if (maxIsActive)
                {
                    FixedV3 temp;
                    FixedV3.Multiply(ref jacobianMaxA, accumulatedImpulse.Y, out temp);
                    FixedV3.Add(ref impulse, ref temp, out impulse);
                }
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                var impulse = new FixedV3();
                if (minIsActive)
                {
                    FixedV3.Multiply(ref jacobianMinB, accumulatedImpulse.X, out impulse);
                }
                if (maxIsActive)
                {
                    FixedV3 temp;
                    FixedV3.Multiply(ref jacobianMaxB, accumulatedImpulse.Y, out temp);
                    FixedV3.Add(ref impulse, ref temp, out impulse);
                }
                connectionB.ApplyAngularImpulse(ref impulse);
            }
        }

        private Fixed64 GetDistanceFromMinimum(Fixed64 angle)
        {
            if (minimumAngle > F64.C0)
            {
                if (angle >= minimumAngle)
                    return angle - minimumAngle;
                if (angle > F64.C0)
                    return MathHelper.TwoPi - minimumAngle + angle;
                return MathHelper.TwoPi - minimumAngle + angle;
            }
            if (angle < minimumAngle)
                return MathHelper.TwoPi - minimumAngle + angle;
            return angle - minimumAngle;
            //else //if (currentAngle >= 0)
            //    return angle - minimumAngle;
        }
    }
}
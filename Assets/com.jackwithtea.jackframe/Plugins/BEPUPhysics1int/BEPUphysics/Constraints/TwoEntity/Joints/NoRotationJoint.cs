using System;
using BEPUphysics.Entities;
using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Constraints.TwoEntity.Joints
{
    /// <summary>
    /// Constrains two entities so that they cannot rotate relative to each other.
    /// </summary>
    public class NoRotationJoint : BEPUJoint, I3DImpulseConstraintWithError, I3DJacobianConstraint
    {
        private FixedV3 accumulatedImpulse;
        private FixedV3 biasVelocity;
        private BEPUMatrix3x3 effectiveMassMatrix;
        private FixedQuaternion initialQuaternionConjugateA;
        private FixedQuaternion initialQuaternionConjugateB;
        private FixedV3 error;

        /// <summary>
        /// Constructs a new constraint which prevents relative angular motion between the two connected bodies.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) and the initial orientations
        /// (InitialOrientationA, InitialOrientationB).
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public NoRotationJoint()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a new constraint which prevents relative angular motion between the two connected bodies.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        public NoRotationJoint(Entity connectionA, Entity connectionB)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;

            initialQuaternionConjugateA = FixedQuaternion.Conjugate(ConnectionA.orientation);
            initialQuaternionConjugateB = FixedQuaternion.Conjugate(ConnectionB.orientation);
        }

        /// <summary>
        /// Gets or sets the initial orientation of the first connected entity.
        /// The constraint will try to maintain the relative orientation between the initialOrientationA and initialOrientationB.
        /// </summary>
        public FixedQuaternion InitialOrientationA
        {
            get { return FixedQuaternion.Conjugate(initialQuaternionConjugateA); }
            set { initialQuaternionConjugateA = FixedQuaternion.Conjugate(value); }
        }

        /// <summary>
        /// Gets or sets the initial orientation of the second connected entity.
        /// The constraint will try to maintain the relative orientation between the initialOrientationA and initialOrientationB.
        /// </summary>
        public FixedQuaternion InitialOrientationB
        {
            get { return FixedQuaternion.Conjugate(initialQuaternionConjugateB); }
            set { initialQuaternionConjugateB = FixedQuaternion.Conjugate(value); }
        }

        #region I3DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        public FixedV3 RelativeVelocity
        {
            get
            {
                FixedV3 velocityDifference;
                FixedV3.Subtract(ref connectionB.angularVelocity, ref connectionA.angularVelocity, out velocityDifference);
                return velocityDifference;
            }
        }

        /// <summary>
        /// Gets the total impulse applied by this constraint.
        /// </summary>
        public FixedV3 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the current constraint error.
        /// </summary>
        public FixedV3 Error
        {
            get { return error; }
        }

        #endregion

        #region I3DJacobianConstraint Members

        /// <summary>
        /// Gets the linear jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobianX">First linear jacobian entry for the first connected entity.</param>
        /// <param name="jacobianY">Second linear jacobian entry for the first connected entity.</param>
        /// <param name="jacobianZ">Third linear jacobian entry for the first connected entity.</param>
        public void GetLinearJacobianA(out FixedV3 jacobianX, out FixedV3 jacobianY, out FixedV3 jacobianZ)
        {
            jacobianX = Toolbox.ZeroVector;
            jacobianY = Toolbox.ZeroVector;
            jacobianZ = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the linear jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobianX">First linear jacobian entry for the second connected entity.</param>
        /// <param name="jacobianY">Second linear jacobian entry for the second connected entity.</param>
        /// <param name="jacobianZ">Third linear jacobian entry for the second connected entity.</param>
        public void GetLinearJacobianB(out FixedV3 jacobianX, out FixedV3 jacobianY, out FixedV3 jacobianZ)
        {
            jacobianX = Toolbox.ZeroVector;
            jacobianY = Toolbox.ZeroVector;
            jacobianZ = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobianX">First angular jacobian entry for the first connected entity.</param>
        /// <param name="jacobianY">Second angular jacobian entry for the first connected entity.</param>
        /// <param name="jacobianZ">Third angular jacobian entry for the first connected entity.</param>
        public void GetAngularJacobianA(out FixedV3 jacobianX, out FixedV3 jacobianY, out FixedV3 jacobianZ)
        {
            jacobianX = Toolbox.RightVector;
            jacobianY = Toolbox.UpVector;
            jacobianZ = Toolbox.BackVector;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobianX">First angular jacobian entry for the second connected entity.</param>
        /// <param name="jacobianY">Second angular jacobian entry for the second connected entity.</param>
        /// <param name="jacobianZ">Third angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out FixedV3 jacobianX, out FixedV3 jacobianY, out FixedV3 jacobianZ)
        {
            jacobianX = Toolbox.RightVector;
            jacobianY = Toolbox.UpVector;
            jacobianZ = Toolbox.BackVector;
        }

        /// <summary>
        /// Gets the mass matrix of the constraint.
        /// </summary>
        /// <param name="outputMassMatrix">Constraint's mass matrix.</param>
        public void GetMassMatrix(out BEPUMatrix3x3 outputMassMatrix)
        {
            outputMassMatrix = effectiveMassMatrix;
        }

        #endregion

        /// <summary>
        /// Applies the corrective impulses required by the constraint.
        /// </summary>
        public override Fixed64 SolveIteration()
        {
            FixedV3 velocityDifference;
            FixedV3.Subtract(ref connectionB.angularVelocity, ref connectionA.angularVelocity, out velocityDifference);
            FixedV3 softnessVector;
            FixedV3.Multiply(ref accumulatedImpulse, softness, out softnessVector);

            FixedV3 lambda;
            FixedV3.Add(ref velocityDifference, ref biasVelocity, out lambda);
            FixedV3.Subtract(ref lambda, ref softnessVector, out lambda);
            BEPUMatrix3x3.Transform(ref lambda, ref effectiveMassMatrix, out lambda);

            FixedV3.Add(ref lambda, ref accumulatedImpulse, out accumulatedImpulse);
            if (connectionA.isDynamic)
            {
                connectionA.ApplyAngularImpulse(ref lambda);
            }
            if (connectionB.isDynamic)
            {
                FixedV3 torqueB;
                FixedV3.Negate(ref lambda, out torqueB);
                connectionB.ApplyAngularImpulse(ref torqueB);
            }

            return Fixed64.Abs(lambda.X) + Fixed64.Abs(lambda.Y) + Fixed64.Abs(lambda.Z);
        }

        /// <summary>
        /// Initializes the constraint for the current frame.
        /// </summary>
        /// <param name="dt">Time between frames.</param>
        public override void Update(Fixed64 dt)
        {
            FixedQuaternion quaternionA;
            FixedQuaternion.Multiply(ref connectionA.orientation, ref initialQuaternionConjugateA, out quaternionA);
            FixedQuaternion quaternionB;
            FixedQuaternion.Multiply(ref connectionB.orientation, ref initialQuaternionConjugateB, out quaternionB);
            FixedQuaternion.Conjugate(ref quaternionB, out quaternionB);
            FixedQuaternion intermediate;
            FixedQuaternion.Multiply(ref quaternionA, ref quaternionB, out intermediate);


            Fixed64 angle;
            FixedV3 axis;
            FixedQuaternion.GetAxisAngleFromQuaternion(ref intermediate, out axis, out angle);

            error.X = axis.X * angle;
            error.Y = axis.Y * angle;
            error.Z = axis.Z * angle;

            Fixed64 errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReduction, out softness);
            errorReduction = -errorReduction;
            biasVelocity.X = errorReduction * error.X;
            biasVelocity.Y = errorReduction * error.Y;
            biasVelocity.Z = errorReduction * error.Z;

            //Ensure that the corrective velocity doesn't exceed the max.
            Fixed64 length = biasVelocity.LengthSquared();
            if (length > maxCorrectiveVelocitySquared)
            {
                Fixed64 multiplier = maxCorrectiveVelocity / Fixed64.Sqrt(length);
                biasVelocity.X *= multiplier;
                biasVelocity.Y *= multiplier;
                biasVelocity.Z *= multiplier;
            }

            BEPUMatrix3x3.Add(ref connectionA.inertiaTensorInverse, ref connectionB.inertiaTensorInverse, out effectiveMassMatrix);
            effectiveMassMatrix.M11 += softness;
            effectiveMassMatrix.M22 += softness;
            effectiveMassMatrix.M33 += softness;
            BEPUMatrix3x3.Invert(ref effectiveMassMatrix, out effectiveMassMatrix);


           
        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //apply accumulated impulse
            if (connectionA.isDynamic)
            {
                connectionA.ApplyAngularImpulse(ref accumulatedImpulse);
            }
            if (connectionB.isDynamic)
            {
                FixedV3 torqueB;
                FixedV3.Negate(ref accumulatedImpulse, out torqueB);
                connectionB.ApplyAngularImpulse(ref torqueB);
            }
        } 
    }
}
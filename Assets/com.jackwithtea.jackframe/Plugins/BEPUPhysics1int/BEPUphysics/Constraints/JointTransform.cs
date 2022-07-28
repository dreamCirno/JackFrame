using System;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.Constraints
{
    /// <summary>
    /// Defines a three dimensional orthonormal basis used by a constraint.
    /// </summary>
    public class JointBasis3D
    {
        internal FixedV3 localPrimaryAxis = FixedV3.Backward;
        internal FixedV3 localXAxis = FixedV3.Right;
        internal FixedV3 localYAxis = FixedV3.Up;
        internal FixedV3 primaryAxis = FixedV3.Backward;
        internal BEPUMatrix3x3 rotationMatrix = BEPUMatrix3x3.Identity;
        internal FixedV3 xAxis = FixedV3.Right;
        internal FixedV3 yAxis = FixedV3.Up;

        /// <summary>
        /// Gets the primary axis of the transform in local space.
        /// </summary>
        public FixedV3 LocalPrimaryAxis
        {
            get { return localPrimaryAxis; }
        }

        /// <summary>
        /// Gets or sets the local transform of the basis.
        /// </summary>
        public BEPUMatrix3x3 LocalTransform
        {
            get
            {
                var toReturn = new BEPUMatrix3x3 {Right = localXAxis, Up = localYAxis, Backward = localPrimaryAxis};
                return toReturn;
            }
            set { SetLocalAxes(value); }
        }

        /// <summary>
        /// Gets the X axis of the transform in local space.
        /// </summary>
        public FixedV3 LocalXAxis
        {
            get { return localXAxis; }
        }

        /// <summary>
        /// Gets the Y axis of the transform in local space.
        /// </summary>
        public FixedV3 LocalYAxis
        {
            get { return localYAxis; }
        }

        /// <summary>
        /// Gets the primary axis of the transform.
        /// </summary>
        public FixedV3 PrimaryAxis
        {
            get { return primaryAxis; }
        }

        /// <summary>
        /// Gets or sets the rotation matrix used by the joint transform to convert local space axes to world space.
        /// </summary>
        public BEPUMatrix3x3 RotationMatrix
        {
            get { return rotationMatrix; }
            set
            {
                rotationMatrix = value;
                ComputeWorldSpaceAxes();
            }
        }

        /// <summary>
        /// Gets or sets the world transform of the basis.
        /// </summary>
        public BEPUMatrix3x3 WorldTransform
        {
            get
            {
                var toReturn = new BEPUMatrix3x3 {Right = xAxis, Up = yAxis, Backward = primaryAxis};
                return toReturn;
            }
            set { SetWorldAxes(value); }
        }

        /// <summary>
        /// Gets the X axis of the transform.
        /// </summary>
        public FixedV3 XAxis
        {
            get { return xAxis; }
        }

        /// <summary>
        /// Gets the Y axis of the transform.
        /// </summary>
        public FixedV3 YAxis
        {
            get { return yAxis; }
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        /// <param name="rotationMatrix">Matrix to use to transform the local axes into world space.</param>
        public void SetLocalAxes(FixedV3 primaryAxis, FixedV3 xAxis, FixedV3 yAxis, BEPUMatrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetLocalAxes(primaryAxis, xAxis, yAxis);
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        public void SetLocalAxes(FixedV3 primaryAxis, FixedV3 xAxis, FixedV3 yAxis)
        {
            if (Fixed64.Abs(FixedV3.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon ||
				Fixed64.Abs(FixedV3.Dot(primaryAxis, yAxis)) > Toolbox.BigEpsilon ||
				Fixed64.Abs(FixedV3.Dot(xAxis, yAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            localPrimaryAxis = FixedV3.Normalize(primaryAxis);
            localXAxis = FixedV3.Normalize(xAxis);
            localYAxis = FixedV3.Normalize(yAxis);
            ComputeWorldSpaceAxes();
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.
        /// The matrix's up vector is used as the y axis.</param>
        public void SetLocalAxes(BEPUMatrix3x3 matrix)
        {
            if (Fixed64.Abs(FixedV3.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon ||
				Fixed64.Abs(FixedV3.Dot(matrix.Backward, matrix.Up)) > Toolbox.BigEpsilon ||
				Fixed64.Abs(FixedV3.Dot(matrix.Right, matrix.Up)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            localPrimaryAxis = FixedV3.Normalize(matrix.Backward);
            localXAxis = FixedV3.Normalize(matrix.Right);
            localYAxis = FixedV3.Normalize(matrix.Up);
            ComputeWorldSpaceAxes();
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        /// <param name="rotationMatrix">Matrix to use to transform the local axes into world space.</param>
        public void SetWorldAxes(FixedV3 primaryAxis, FixedV3 xAxis, FixedV3 yAxis, BEPUMatrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetWorldAxes(primaryAxis, xAxis, yAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        public void SetWorldAxes(FixedV3 primaryAxis, FixedV3 xAxis, FixedV3 yAxis)
        {
            if (Fixed64.Abs(FixedV3.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon ||
				Fixed64.Abs(FixedV3.Dot(primaryAxis, yAxis)) > Toolbox.BigEpsilon ||
				Fixed64.Abs(FixedV3.Dot(xAxis, yAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            this.primaryAxis = FixedV3.Normalize(primaryAxis);
            this.xAxis = FixedV3.Normalize(xAxis);
            this.yAxis = FixedV3.Normalize(yAxis);
            BEPUMatrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            BEPUMatrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
            BEPUMatrix3x3.TransformTranspose(ref this.yAxis, ref rotationMatrix, out localYAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.
        /// The matrix's up vector is used as the y axis.</param>
        public void SetWorldAxes(BEPUMatrix3x3 matrix)
        {
            if (Fixed64.Abs(FixedV3.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon ||
				Fixed64.Abs(FixedV3.Dot(matrix.Backward, matrix.Up)) > Toolbox.BigEpsilon ||
				Fixed64.Abs(FixedV3.Dot(matrix.Right, matrix.Up)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            primaryAxis = FixedV3.Normalize(matrix.Backward);
            xAxis = FixedV3.Normalize(matrix.Right);
            yAxis = FixedV3.Normalize(matrix.Up);
            BEPUMatrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            BEPUMatrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
            BEPUMatrix3x3.TransformTranspose(ref this.yAxis, ref rotationMatrix, out localYAxis);
        }

        internal void ComputeWorldSpaceAxes()
        {
            BEPUMatrix3x3.Transform(ref localPrimaryAxis, ref rotationMatrix, out primaryAxis);
            BEPUMatrix3x3.Transform(ref localXAxis, ref rotationMatrix, out xAxis);
            BEPUMatrix3x3.Transform(ref localYAxis, ref rotationMatrix, out yAxis);
        }
    }

    /// <summary>
    /// Defines a two axes which are perpendicular to each other used by a constraint.
    /// </summary>
    public class JointBasis2D
    {
        internal FixedV3 localPrimaryAxis = FixedV3.Backward;
        internal FixedV3 localXAxis = FixedV3.Right;
        internal FixedV3 primaryAxis = FixedV3.Backward;
        internal BEPUMatrix3x3 rotationMatrix = BEPUMatrix3x3.Identity;
        internal FixedV3 xAxis = FixedV3.Right;

        /// <summary>
        /// Gets the primary axis of the transform in local space.
        /// </summary>
        public FixedV3 LocalPrimaryAxis
        {
            get { return localPrimaryAxis; }
        }

        /// <summary>
        /// Gets the X axis of the transform in local space.
        /// </summary>
        public FixedV3 LocalXAxis
        {
            get { return localXAxis; }
        }

        /// <summary>
        /// Gets the primary axis of the transform.
        /// </summary>
        public FixedV3 PrimaryAxis
        {
            get { return primaryAxis; }
        }

        /// <summary>
        /// Gets or sets the rotation matrix used by the joint transform to convert local space axes to world space.
        /// </summary>
        public BEPUMatrix3x3 RotationMatrix
        {
            get { return rotationMatrix; }
            set
            {
                rotationMatrix = value;
                ComputeWorldSpaceAxes();
            }
        }

        /// <summary>
        /// Gets the X axis of the transform.
        /// </summary>
        public FixedV3 XAxis
        {
            get { return xAxis; }
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="rotationMatrix">Matrix to use to transform the local axes into world space.</param>
        public void SetLocalAxes(FixedV3 primaryAxis, FixedV3 xAxis, BEPUMatrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetLocalAxes(primaryAxis, xAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        public void SetLocalAxes(FixedV3 primaryAxis, FixedV3 xAxis)
        {
            if (Fixed64.Abs(FixedV3.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");

            localPrimaryAxis = FixedV3.Normalize(primaryAxis);
            localXAxis = FixedV3.Normalize(xAxis);
            ComputeWorldSpaceAxes();
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.</param>
        public void SetLocalAxes(BEPUMatrix3x3 matrix)
        {
            if (Fixed64.Abs(FixedV3.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");
            localPrimaryAxis = FixedV3.Normalize(matrix.Backward);
            localXAxis = FixedV3.Normalize(matrix.Right);
            ComputeWorldSpaceAxes();
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="rotationMatrix">Matrix to use to transform the local axes into world space.</param>
        public void SetWorldAxes(FixedV3 primaryAxis, FixedV3 xAxis, BEPUMatrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetWorldAxes(primaryAxis, xAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        public void SetWorldAxes(FixedV3 primaryAxis, FixedV3 xAxis)
        {
            if (Fixed64.Abs(FixedV3.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");
            this.primaryAxis = FixedV3.Normalize(primaryAxis);
            this.xAxis = FixedV3.Normalize(xAxis);
            BEPUMatrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            BEPUMatrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.</param>
        public void SetWorldAxes(BEPUMatrix3x3 matrix)
        {
            if (Fixed64.Abs(FixedV3.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");
            primaryAxis = FixedV3.Normalize(matrix.Backward);
            xAxis = FixedV3.Normalize(matrix.Right);
            BEPUMatrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            BEPUMatrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
        }

        internal void ComputeWorldSpaceAxes()
        {
            BEPUMatrix3x3.Transform(ref localPrimaryAxis, ref rotationMatrix, out primaryAxis);
            BEPUMatrix3x3.Transform(ref localXAxis, ref rotationMatrix, out xAxis);
        }
    }
}
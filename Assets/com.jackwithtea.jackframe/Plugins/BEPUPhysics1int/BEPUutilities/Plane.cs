using FixMath.NET;

namespace BEPUutilities
{
    /// <summary>
    /// Provides XNA-like plane functionality.
    /// </summary>
    public struct BEPUPlane
    {
        /// <summary>
        /// Normal of the plane.
        /// </summary>
        public FixedV3 Normal;
        /// <summary>
        /// Negative distance to the plane from the origin along the normal.
        /// </summary>
        public Fixed64 D;


        /// <summary>
        /// Constructs a new plane.
        /// </summary>
        /// <param name="position">A point on the plane.</param>
        /// <param name="normal">The normal of the plane.</param>
        public BEPUPlane(ref FixedV3 position, ref FixedV3 normal)
        {
            Fixed64 d;
            FixedV3.Dot(ref position, ref normal, out d);
            D = -d;
            Normal = normal;
        }


        /// <summary>
        /// Constructs a new plane.
        /// </summary>
        /// <param name="position">A point on the plane.</param>
        /// <param name="normal">The normal of the plane.</param>
        public BEPUPlane(FixedV3 position, FixedV3 normal)
            : this(ref position, ref normal)
        {

        }


        /// <summary>
        /// Constructs a new plane.
        /// </summary>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="d">Negative distance to the plane from the origin along the normal.</param>
        public BEPUPlane(FixedV3 normal, Fixed64 d)
            : this(ref normal, d)
        {
        }

        /// <summary>
        /// Constructs a new plane.
        /// </summary>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="d">Negative distance to the plane from the origin along the normal.</param>
        public BEPUPlane(ref FixedV3 normal, Fixed64 d)
        {
            this.Normal = normal;
            this.D = d;
        }

        /// <summary>
        /// Gets the dot product of the position offset from the plane along the plane's normal.
        /// </summary>
        /// <param name="v">Position to compute the dot product of.</param>
        /// <param name="dot">Dot product.</param>
        public void DotCoordinate(ref FixedV3 v, out Fixed64 dot)
        {
            dot = Normal.X * v.X + Normal.Y * v.Y + Normal.Z * v.Z + D;
        }
    }
}

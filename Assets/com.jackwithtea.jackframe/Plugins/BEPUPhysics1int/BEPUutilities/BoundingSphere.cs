using FixMath.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BEPUutilities
{    
    /// <summary>
    /// Provides XNA-like bounding sphere functionality.
    /// </summary>
    public struct BoundingSphere
    {
        /// <summary>
        /// Radius of the sphere.
        /// </summary>
        public Fixed64 Radius;
        /// <summary>
        /// Location of the center of the sphere.
        /// </summary>
        public FixedV3 Center;

        /// <summary>
        /// Constructs a new bounding sphere.
        /// </summary>
        /// <param name="center">Location of the center of the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        public BoundingSphere(FixedV3 center, Fixed64 radius)
        {
            this.Center = center;
            this.Radius = radius;
        }
    }
}

﻿

using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.Paths
{
    /// <summary>
    /// Wrapper around a 3d position curve that specifies a specific velocity at which to travel.
    /// </summary>
    public class ConstantLinearSpeedCurve : ConstantSpeedCurve<FixedV3>
    {
        /// <summary>
        /// Constructs a new constant speed curve.
        /// </summary>
        /// <param name="speed">Speed to maintain while traveling around a curve.</param>
        /// <param name="curve">Curve to wrap.</param>
        public ConstantLinearSpeedCurve(Fixed64 speed, Curve<FixedV3> curve)
            : base(speed, curve)
        {
        }

        /// <summary>
        /// Constructs a new constant speed curve.
        /// </summary>
        /// <param name="speed">Speed to maintain while traveling around a curve.</param>
        /// <param name="curve">Curve to wrap.</param>
        /// <param name="sampleCount">Number of samples to use when constructing the wrapper curve.
        /// More samples increases the accuracy of the speed requirement at the cost of performance.</param>
        public ConstantLinearSpeedCurve(Fixed64 speed, Curve<FixedV3> curve, int sampleCount)
            : base(speed, curve, sampleCount)
        {
        }

        protected override Fixed64 GetDistance(FixedV3 start, FixedV3 end)
        {
            Fixed64 distance;
            FixedV3.Distance(ref start, ref end, out distance);
            return distance;
        }
    }
}
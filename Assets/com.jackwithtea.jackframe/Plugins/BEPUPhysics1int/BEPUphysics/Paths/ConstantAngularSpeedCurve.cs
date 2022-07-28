using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.Paths
{
    /// <summary>
    /// Wrapper around an orientation curve that specifies a specific velocity at which to travel.
    /// </summary>
    public class ConstantAngularSpeedCurve : ConstantSpeedCurve<FixedQuaternion>
    {
        /// <summary>
        /// Constructs a new constant speed curve.
        /// </summary>
        /// <param name="speed">Speed to maintain while traveling around a curve.</param>
        /// <param name="curve">Curve to wrap.</param>
        public ConstantAngularSpeedCurve(Fixed64 speed, Curve<FixedQuaternion> curve)
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
        public ConstantAngularSpeedCurve(Fixed64 speed, Curve<FixedQuaternion> curve, int sampleCount)
            : base(speed, curve, sampleCount)
        {
        }

        protected override Fixed64 GetDistance(FixedQuaternion start, FixedQuaternion end)
        {
            FixedQuaternion.Conjugate(ref end, out end);
            FixedQuaternion.Multiply(ref end, ref start, out end);
            return FixedQuaternion.GetAngleFromQuaternion(ref end);
        }
    }
}
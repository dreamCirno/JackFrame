using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.Paths
{
    /// <summary>
    /// Wraps a curve that is traveled along with arbitrary defined angular speed.
    /// </summary>
    /// <remarks>
    /// The speed curve should be designed with the wrapped curve's times in mind.
    /// Speeds will be sampled based on the wrapped curve's interval.</remarks>
    public class VariableAngularSpeedCurve : VariableSpeedCurve<FixedQuaternion>
    {
        /// <summary>
        /// Constructs a new variable speed curve.
        /// </summary>
        /// <param name="speedCurve">Curve defining speeds to use.</param>
        /// <param name="curve">Curve to wrap.</param>
        public VariableAngularSpeedCurve(Path<Fixed64> speedCurve, Curve<FixedQuaternion> curve)
            : base(speedCurve, curve)
        {
        }

        /// <summary>
        /// Constructs a new variable speed curve.
        /// </summary>
        /// <param name="speedCurve">Curve defining speeds to use.</param>
        /// <param name="curve">Curve to wrap.</param>
        /// <param name="sampleCount">Number of samples to use when constructing the wrapper curve.
        /// More samples increases the accuracy of the speed requirement at the cost of performance.</param>
        public VariableAngularSpeedCurve(Path<Fixed64> speedCurve, Curve<FixedQuaternion> curve, int sampleCount)
            : base(speedCurve, curve, sampleCount)
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
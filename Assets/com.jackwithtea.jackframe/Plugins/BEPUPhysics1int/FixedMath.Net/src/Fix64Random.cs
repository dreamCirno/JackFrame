using System;

namespace FixMath.NET
{
	public class Fix64Random
    {
        private Random random;

        public Fix64Random(int seed)
        {
            random = new Random(seed);
        }

        public Fixed64 Next()
        {
            Fixed64 result = new Fixed64();
            result.RawValue = (uint)random.Next(int.MinValue, int.MaxValue);
            return result;
        }

        public Fixed64 NextInt(int maxValue)
        {
            return random.Next(maxValue);
        }
    }
}

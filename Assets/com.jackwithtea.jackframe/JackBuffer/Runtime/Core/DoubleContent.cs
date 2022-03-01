using System.Runtime.InteropServices;

namespace JackBuffer {
    
    [StructLayout(LayoutKind.Explicit)]
    internal struct DoubleContent {
        [FieldOffset(0)]
        public double doubleValue;
        [FieldOffset(0)]
        public ulong ulongValue;
    }
}
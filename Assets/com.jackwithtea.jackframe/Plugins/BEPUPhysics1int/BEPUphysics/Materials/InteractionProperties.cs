using FixMath.NET;

namespace BEPUPhysics1int.Materials
{
    ///<summary>
    /// Contains the blended friction and bounciness of a pair of objects.
    ///</summary>
    public struct InteractionProperties
    {
        ///<summary>
        /// Kinetic friction between the pair of objects.
        ///</summary>
        public Fixed64 KineticFriction;
        ///<summary>
        /// Static friction between the pair of objects.
        ///</summary>
        public Fixed64 StaticFriction;
        ///<summary>
        /// Bounciness between the pair of objects.
        ///</summary>
        public Fixed64 Bounciness;
    }
}

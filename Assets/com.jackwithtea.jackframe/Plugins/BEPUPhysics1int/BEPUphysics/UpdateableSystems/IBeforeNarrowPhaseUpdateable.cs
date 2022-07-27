using FixMath.NET;

namespace BEPUPhysics1int.UpdateableSystems
{
    ///<summary>
    /// Defines an object which is updated by the space before the narrow phase runs.
    ///</summary>
    public interface IBeforeNarrowPhaseUpdateable : ISpaceUpdateable
    {
        ///<summary>
        /// Updates the updateable before the narrow phase.
        ///</summary>
        ///<param name="dt">Time step duration.</param>
        void Update(Fixed64 dt);

    }
}

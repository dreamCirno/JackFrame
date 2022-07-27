using BEPUPhysics1int.BroadPhaseEntries;

namespace BEPUPhysics1int.BroadPhaseSystems
{
    ///<summary>
    /// Requires that a class own a BroadPhaseEntry.
    ///</summary>
    public interface IBroadPhaseEntryOwner
    {
        ///<summary>
        /// Gets the broad phase entry associated with this object.
        ///</summary>
        BroadPhaseEntry Entry { get; }
    }
}

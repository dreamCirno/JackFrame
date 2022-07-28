using FixMath.NET;

namespace BEPUPhysics1int.UpdateableSystems
{
    ///<summary>
    /// Defines an object which is updated by the space at the end of the frame.
    ///</summary>
    public interface IEndOfFrameUpdateable : ISpaceUpdateable
    {
        /// <summary>
        /// Updates the object at the end of the frame.
        /// </summary>
        /// <param name="dt">Time step duration.</param>
        void Update(Fixed64 dt);

    }
}

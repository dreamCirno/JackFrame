﻿using FixMath.NET;

namespace BEPUPhysics1int.UpdateableSystems
{
    ///<summary>
    /// Defines an object which is updated by the space before the solver runs.
    ///</summary>
    public interface IBeforeSolverUpdateable : ISpaceUpdateable
    {
        ///<summary>
        /// Updates the updateable before the solver.
        ///</summary>
        ///<param name="dt">Time step duration.</param>
        void Update(Fixed64 dt);

    }
}

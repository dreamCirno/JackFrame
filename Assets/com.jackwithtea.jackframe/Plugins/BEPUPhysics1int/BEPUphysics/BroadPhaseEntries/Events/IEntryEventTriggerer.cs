﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUPhysics1int.BroadPhaseEntries;
using BEPUPhysics1int.BroadPhaseSystems;
using BEPUPhysics1int.NarrowPhaseSystems.Pairs;

namespace BEPUPhysics1int.BroadPhaseEntries.Events
{
    /// <summary>
    /// Manages triggers for events in an EntryEventManager.
    /// </summary>
    public interface IEntryEventTriggerer
    {
        /// <summary>
        /// Fires the event manager's pair creation events.
        /// </summary>
        /// <param name="other">Other entry involved in the pair.</param>
        /// <param name="collisionPair">Narrow phase pair governing the two objects.</param>
        void OnPairCreated(BroadPhaseEntry other, NarrowPhasePair collisionPair);

        /// <summary>
        /// Fires the event manager's pair removal events.
        /// </summary>
        /// <param name="other">Other entry involved in the pair.</param>
        void OnPairRemoved(BroadPhaseEntry other);

        /// <summary>
        /// Fires the event manager's pair updated events.
        /// </summary>
        /// <param name="other">Other entry involved in the pair.</param>
        /// <param name="collisionPair">Narrow phase pair governing the two objects.</param>
        void OnPairUpdated(BroadPhaseEntry other, NarrowPhasePair collisionPair);
    }
}

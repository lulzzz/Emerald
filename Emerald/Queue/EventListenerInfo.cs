using System;

namespace Emerald.Queue
{
    internal sealed class EventListenerInfo
    {
        public EventListenerInfo(Guid cycleId, DateTime startedAt)
        {
            CycleId = cycleId;
            StartedAt = startedAt;
        }

        public Guid CycleId { get; }
        public DateTime StartedAt { get; }
    }
}
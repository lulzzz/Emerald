using Akka.Routing;
using Emerald.Core;
using System;

namespace Emerald.Queue
{
    internal sealed class Event : IConsistentHashable
    {
        public Event(long id, string type, string body, string consistentHashKey, DateTime readAt)
        {
            Id = id;
            Type = type;
            Body = body;
            ConsistentHashKey = consistentHashKey;
            ReadAt = readAt;
        }

        public long Id { get; }
        public string Type { get; }
        public string Body { get; }
        public object ConsistentHashKey { get; }
        public DateTime ReadAt { get; }

        public EventListenerInfo Listener { get; private set; }

        public void SetListener(EventListenerInfo listener)
        {
            Listener = listener;
        }
    }

    internal sealed class EventListenerInfo
    {
        public EventListenerInfo(Guid cycleId, long[] events, DateTime startedAt)
        {
            CycleId = cycleId;
            Events = events;
            StartedAt = startedAt;
        }

        public Guid CycleId { get; }
        public long[] Events { get; }
        public DateTime StartedAt { get; }
    }

    internal sealed class EventHandlerInfo
    {
        internal const string ErrorResult = "Error";
        internal const string SuccessResult = "Success";

        public EventHandlerInfo(ICommandInfo[] commands, string result, DateTime startedAt, string type)
        {
            Commands = commands;
            HandlingTime = $"{Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)}ms";
            Result = result;
            StartedAt = startedAt;
            Type = type;
        }

        public ICommandInfo[] Commands { get; }
        public string HandlingTime { get; }
        public string Result { get; }
        public DateTime StartedAt { get; }
        public string Type { get; }
    }
}
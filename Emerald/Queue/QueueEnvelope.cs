using Akka.Routing;

namespace Emerald.Queue
{
    internal sealed class QueueEnvelope : IConsistentHashable
    {
        public QueueEnvelope(Event @event, EventListenerInfo listener)
        {
            Event = @event;
            Listener = listener;
        }

        public Event Event { get; }
        public EventListenerInfo Listener { get; }
        public object ConsistentHashKey => Event.ConsistentHashKey;
    }
}
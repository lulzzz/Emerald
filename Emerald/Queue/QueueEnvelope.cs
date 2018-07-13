using Akka.Routing;

namespace Emerald.Queue
{
    internal sealed class QueueEnvelope : IConsistentHashable
    {
        public QueueEnvelope(Event @event, EventProcessingLogBuilder eventProcessingLogBuilder)
        {
            Event = @event;
            EventProcessingLogBuilder = eventProcessingLogBuilder;
        }

        public Event Event { get; }
        public EventProcessingLogBuilder EventProcessingLogBuilder { get; }
        public object ConsistentHashKey => Event.ConsistentHashKey;
    }
}
using Akka.Routing;
using Emerald.Utils;
using System;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public sealed class EventPublisher
    {
        private readonly QueueDbAccessManager _queueDbAccessManager;

        internal EventPublisher(QueueDbAccessManager queueDbAccessManager)
        {
            _queueDbAccessManager = queueDbAccessManager ?? throw new ArgumentNullException(nameof(queueDbAccessManager));
        }

        public async Task Publish(object @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var type = @event.GetType().Name;
            var body = @event.ToJson();
            var consistentHashKey = @event is IConsistentHashable consistentHashable ? consistentHashable.ConsistentHashKey?.ToString() : null;

            await _queueDbAccessManager.AddEvent(type, body, consistentHashKey);
        }

        public static async Task<EventPublisher> Create(string applicationName, string connectionString)
        {
            var queueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
            await queueDbAccessManager.CreateQueueDbIfNeeded();
            return new EventPublisher(queueDbAccessManager);
        }
    }
}
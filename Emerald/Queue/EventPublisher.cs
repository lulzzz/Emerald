using Akka.Routing;
using Emerald.Utils;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public sealed class EventPublisher : IEventPublisher
    {
        private readonly QueueDbAccessManager _queueDbAccessManager;

        internal EventPublisher(QueueDbAccessManager queueDbAccessManager)
        {
            _queueDbAccessManager = queueDbAccessManager;
        }

        public async Task Publish(object @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            if (_queueDbAccessManager == null) return;
            var type = @event.GetType().Name;
            var body = JsonHelper.Serialize(@event, Formatting.None);
            var consistentHashKey = @event is IConsistentHashable consistentHashable ? consistentHashable.ConsistentHashKey?.ToString() : null;
            await _queueDbAccessManager.AddEvent(type, body, consistentHashKey);
        }

        public static async Task<IEventPublisher> Create(string applicationName, string connectionString)
        {
            var queueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
            await queueDbAccessManager.CreateQueueDbIfNeeded();
            return new EventPublisher(queueDbAccessManager);
        }
    }
}
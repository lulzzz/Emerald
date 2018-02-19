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
            await _queueDbAccessManager.AddEvent(@event.GetType().Name, JsonConvert.SerializeObject(@event));
        }

        public static IEventPublisher Create(string applicationName, string connectionString)
        {
            var queueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
            queueDbAccessManager.CreateQueueDbIfNeeded();
            return new EventPublisher(queueDbAccessManager);
        }
    }
}
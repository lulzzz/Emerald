using Emerald.Common;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public sealed class EventPublisher : IEventPublisher
    {
        private readonly QueueDbAccessManager _queueDbAccessManager;

        internal EventPublisher()
        {
        }
        internal EventPublisher(QueueDbAccessManager queueDbAccessManager)
        {
            _queueDbAccessManager = queueDbAccessManager;
        }

        public async Task Publish(object @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var queueDbAccessManager = _queueDbAccessManager ?? Registry.QueueDbAccessManager;
            await queueDbAccessManager.AddEvent(@event.GetType().FullName, JsonConvert.SerializeObject(@event));
        }

        public static IEventPublisher Create(string applicationName, string connectionString)
        {
            var queueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
            queueDbAccessManager.CreateQueueDbIfNeeded();
            return new EventPublisher(queueDbAccessManager);
        }
    }
}
using Emerald.Utils;
using System;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public sealed class EventPublisher : IEventPublisher
    {
        private readonly QueueDbAccessManager _queueDbAccessManager;

        internal EventPublisher(QueueDbAccessManager queueDbAccessManager)
        {
            _queueDbAccessManager = queueDbAccessManager ?? throw new ArgumentNullException(nameof(queueDbAccessManager));
        }

        public Task Publish(object @event)
        {
            return Publish(null, @event);
        }
        public async Task Publish(string consistentHashKey, object @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var type = @event.GetType().Name;
            var body = @event.ToJson();
            await _queueDbAccessManager.AddEvent(type, body, consistentHashKey);
        }

        public static async Task<IEventPublisher> Create(string applicationName, string connectionString)
        {
            var queueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
            await queueDbAccessManager.CreateQueueDbIfNeeded();
            return new EventPublisher(queueDbAccessManager);
        }
    }

    public interface IEventPublisher
    {
        Task Publish(object @event);
        Task Publish(string consistentHashKey, object @event);
    }
}
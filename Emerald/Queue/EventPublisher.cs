using Emerald.Common;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public sealed class EventPublisher
    {
        internal EventPublisher()
        {
        }

        public async Task Publish(object @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            await Registry.QueueDbAccessManager.AddEvent(@event.GetType().FullName, JsonConvert.SerializeObject(@event));
        }
    }
}
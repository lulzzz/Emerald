using System;
using System.Collections.Generic;
using System.Linq;

namespace Emerald.Queue
{
    public abstract class EventListener
    {
        private readonly EventListenerConfig _config = new EventListenerConfig();

        protected abstract void Configure(EventListenerConfig config);

        internal void Initialize()
        {
            Configure(_config);
        }

        internal void Handle(object @event)
        {
            _config.EventHandlerDictionary[@event.GetType()](@event);
        }

        internal List<Type> GetEventTypes()
        {
            return _config.EventHandlerDictionary.Keys.ToList();
        }
    }
}
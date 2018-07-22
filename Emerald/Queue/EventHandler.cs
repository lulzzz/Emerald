using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public abstract class EventHandler
    {
        private readonly EventHandlerConfig _config = new EventHandlerConfig();

        protected abstract void Configure(EventHandlerConfig config);

        internal void Initialize()
        {
            Configure(_config);
        }
        internal Task Handle(object @event)
        {
            return _config.EventHandlerDictionary[@event.GetType()](@event);
        }
        internal List<Type> GetEventTypes()
        {
            return _config.EventHandlerDictionary.Keys.ToList();
        }
    }

    public sealed class EventHandlerConfig
    {
        internal EventHandlerConfig()
        {
        }

        internal Dictionary<Type, Func<object, Task>> EventHandlerDictionary { get; } = new Dictionary<Type, Func<object, Task>>();

        public void RegisterEventHandler<T>(Func<T, Task> handler)
        {
            EventHandlerDictionary.Add(typeof(T), e => handler((T)e));
        }
    }
}
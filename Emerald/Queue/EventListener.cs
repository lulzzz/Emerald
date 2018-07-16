using Emerald.Abstractions;
using Emerald.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public abstract class EventListener
    {
        private readonly EventListenerConfig _config = new EventListenerConfig();

        protected internal CommandExecutor CommandExecutor { get; internal set; }
        protected internal IServiceScopeFactory ServiceScopeFactory { get; internal set; }
        protected internal ITransactionScopeFactory TransactionScopeFactory { get; internal set; }

        protected abstract void Configure(EventListenerConfig config);

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
}
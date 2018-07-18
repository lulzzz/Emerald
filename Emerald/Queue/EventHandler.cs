using Emerald.Abstractions;
using Emerald.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public abstract class EventHandler
    {
        private readonly EventHandlerConfig _config = new EventHandlerConfig();

        protected internal CommandExecutor CommandExecutor { get; internal set; }
        protected internal IServiceScopeFactory ServiceScopeFactory { get; internal set; }
        protected internal ITransactionScopeFactory TransactionScopeFactory { get; internal set; }

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
}
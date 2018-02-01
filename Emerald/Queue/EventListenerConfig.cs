using System;
using System.Collections.Generic;

namespace Emerald.Queue
{
    public sealed class EventListenerConfig
    {
        internal EventListenerConfig()
        {
        }

        internal Dictionary<Type, Action<object>> EventHandlerDictionary { get; } = new Dictionary<Type, Action<object>>();

        public void RegisterEventHandler<T>(Action<T> handler)
        {
            EventHandlerDictionary.Add(typeof(T), e => handler((T)e));
        }
    }
}
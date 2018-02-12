using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public sealed class EventListenerConfig
    {
        internal EventListenerConfig()
        {
        }

        internal Dictionary<Type, Func<object, Task>> EventHandlerDictionary { get; } = new Dictionary<Type, Func<object, Task>>();

        public void RegisterEventHandler<T>(Func<T, Task> handler)
        {
            EventHandlerDictionary.Add(typeof(T), e => handler((T)e));
        }
    }
}
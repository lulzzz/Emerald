using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emerald.Core
{
    public sealed class CommandHandlerConfig
    {
        internal CommandHandlerConfig()
        {
        }

        internal Dictionary<Type, Func<object, Task<object>>> CommandHandlerDictionary { get; } = new Dictionary<Type, Func<object, Task<object>>>();

        public void RegisterCommandHandler<T>(Func<T, Task> handler) where T : Command
        {
            CommandHandlerDictionary.Add(typeof(T), cmd => { handler((T)cmd); return null; });
        }
        public void RegisterCommandHandler<TCommand, TResult>(Func<TCommand, Task<TResult>> handler) where TCommand : Command
        {
            CommandHandlerDictionary.Add(typeof(TCommand), cmd => handler((TCommand)cmd).ContinueWith(t => (object)t.Result));
        }
    }
}
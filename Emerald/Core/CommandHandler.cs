using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Core
{
    public abstract class CommandHandler
    {
        private readonly CommandHandlerConfig _config = new CommandHandlerConfig();

        protected abstract void Configure(CommandHandlerConfig config);

        internal void Initialize()
        {
            Configure(_config);
        }
        internal Task<object> Handle(object command)
        {
            return _config.CommandHandlerDictionary[command.GetType()](command);
        }
        internal List<Type> GetCommandTypes()
        {
            return _config.CommandHandlerDictionary.Keys.ToList();
        }
    }

    public sealed class CommandHandlerConfig
    {
        internal CommandHandlerConfig()
        {
        }

        internal Dictionary<Type, Func<object, Task<object>>> CommandHandlerDictionary { get; } = new Dictionary<Type, Func<object, Task<object>>>();

        public void RegisterCommandHandler<T>(Func<T, Task> handler) where T : Command
        {
            CommandHandlerDictionary.Add(typeof(T), cmd => { handler((T)cmd); return Task.FromResult<object>(null); });
        }
        public void RegisterCommandHandler<TCommand, TResult>(Func<TCommand, Task<TResult>> handler) where TCommand : Command
        {
            CommandHandlerDictionary.Add(typeof(TCommand), cmd => handler((TCommand)cmd).ContinueWith(t => (object)t.Result));
        }
    }
}
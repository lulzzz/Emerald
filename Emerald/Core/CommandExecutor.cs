using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emerald.Core
{
    public sealed class CommandExecutor
    {
        private readonly Dictionary<Type, IActorRef> _commandHandlerDictionary;
        private readonly List<CommandInfo> _commandInfoList = new List<CommandInfo>();

        internal CommandExecutor(Dictionary<Type, IActorRef> commandHandlerDictionary)
        {
            _commandHandlerDictionary = commandHandlerDictionary;
        }

        public async Task<T> Execute<T>(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var resultTask = _commandHandlerDictionary[command.GetType()].Ask<CommandExecutionResult>(command);
            var result = await resultTask;

            _commandInfoList.Add(result.Info);

            if (result.Exception != null)
            {
                throw result.Exception;
            }

            return (T)result.Output;
        }
        public async Task Execute(Command command)
        {
            await Execute<object>(command);
        }

        public CommandInfo[] GetInfo() => _commandInfoList.ToArray();
    }
}
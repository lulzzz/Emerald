using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emerald.Core
{
    internal sealed class CommandExecutor : ICommandExecutor
    {
        private string _correlationId;
        private readonly Dictionary<Type, Tuple<Type, IActorRef>> _commandHandlerDictionary;
        private readonly List<ICommandInfo> _commandList = new List<ICommandInfo>();

        internal CommandExecutor(Dictionary<Type, Tuple<Type, IActorRef>> commandHandlerDictionary)
        {
            _commandHandlerDictionary = commandHandlerDictionary;
        }

        public async Task<T> Execute<T>(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            command.SetCorrelationId(_correlationId);

            await _commandHandlerDictionary[command.GetType()].Item2.Ask<Command>(command);

            _commandList.Add(command);

            if (command.AsCommandInfo().Exception != null)
            {
                throw command.AsCommandInfo().Exception;
            }

            return (T)command.AsCommandInfo().Output;
        }
        public Task Execute(Command command)
        {
            return Execute<object>(command);
        }

        public ICommandInfo[] GetCommands() => _commandList.ToArray();
        public void SetCorrelationId(string correlationId)
        {
            _correlationId = correlationId;
        }
    }

    public interface ICommandExecutor
    {
        Task<T> Execute<T>(Command command);
        Task Execute(Command command);
        ICommandInfo[] GetCommands();
        void SetCorrelationId(string correlationId);
    }
}
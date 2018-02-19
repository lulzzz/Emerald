using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emerald.Core
{
    public sealed class CommandExecutor
    {
        private readonly ActorSystem _actorSystem;
        private readonly Dictionary<Type, IActorRef> _commandHandlerDictionary;

        internal CommandExecutor(ActorSystem actorSystem, Dictionary<Type, IActorRef> commandHandlerDictionary)
        {
            _actorSystem = actorSystem;
            _commandHandlerDictionary = commandHandlerDictionary;
        }

        public async Task<T> Execute<T>(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandExecutionResultActor = _actorSystem.ActorOf(Props.Create(() => new CommandExecutionResultActor(command.Id)));
            _actorSystem.EventStream.Subscribe(commandExecutionResultActor, typeof(CommandExecutionResult));
            _commandHandlerDictionary[command.GetType()].Tell(command);

            try
            {
                var result = await commandExecutionResultActor.Ask<CommandExecutionResult>(command.Id);
                if (result.Exception != null) throw result.Exception;
                return (T)result.Output;
            }
            finally
            {
                _actorSystem.EventStream.Unsubscribe(commandExecutionResultActor);
                commandExecutionResultActor.Tell(PoisonPill.Instance);
            }
        }

        public async Task Execute(Command command)
        {
            await Execute<object>(command);
        }
    }
}
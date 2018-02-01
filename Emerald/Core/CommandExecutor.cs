using Akka.Actor;
using Emerald.Common;
using System;
using System.Threading.Tasks;

namespace Emerald.Core
{
    public sealed class CommandExecutor
    {
        internal CommandExecutor()
        {
        }

        public async Task<T> Execute<T>(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandExecutionResultActor = Registry.ActorSystem.ActorOf(Props.Create(() => new CommandExecutionResultActor(command.Id)));
            Registry.ActorSystem.EventStream.Subscribe(commandExecutionResultActor, typeof(CommandExecutionResult));
            Registry.CommandHandlerActorDictionary[command.GetType()].Tell(command);

            try
            {
                var result = await commandExecutionResultActor.Ask<CommandExecutionResult>(command.Id);
                if (result.Exception != null) throw result.Exception;
                return (T)result.Output;
            }
            finally
            {
                Registry.ActorSystem.EventStream.Unsubscribe(commandExecutionResultActor);
                commandExecutionResultActor.Tell(PoisonPill.Instance);
            }
        }

        public async Task Execute(Command command)
        {
            await Execute<object>(command);
        }
    }
}
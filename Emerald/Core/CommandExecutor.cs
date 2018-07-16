using Akka.Actor;
using Akka.Routing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emerald.Core
{
    public sealed class CommandExecutor
    {
        private readonly Dictionary<Type, IActorRef> _commandHandlerDictionary;

        internal CommandExecutor(Dictionary<Type, IActorRef> commandHandlerDictionary)
        {
            _commandHandlerDictionary = commandHandlerDictionary;
        }

        public async Task<T> Execute<T>(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var envelope = new CommandEnvelope(command, new CommandProcessingLogBuilder());
            envelope.CommandProcessingLogBuilder.Start();
            envelope.CommandProcessingLogBuilder.SetEventInfo(command.Id, command.GetType().Name, ((IConsistentHashable)command).ConsistentHashKey?.ToString());

            var resultTask = _commandHandlerDictionary[command.GetType()].Ask<CommandExecutionResult>(envelope);
            envelope.CommandProcessingLogBuilder.CommandSent();

            var result = await resultTask;
            envelope.CommandProcessingLogBuilder.ResultReceived();

            if (result.Exception != null)
            {
                envelope.CommandProcessingLogBuilder.SetMessage("Command handled with error.");
                Log.Error(result.Exception, envelope.CommandProcessingLogBuilder.Build());
                throw result.Exception;
            }

            envelope.CommandProcessingLogBuilder.SetMessage("Command handled successfully.");
            Log.Information(envelope.CommandProcessingLogBuilder.Build());

            return (T)result.Output;
        }

        public async Task Execute(Command command)
        {
            await Execute<object>(command);
        }
    }
}
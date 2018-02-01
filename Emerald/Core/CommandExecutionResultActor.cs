using Akka.Actor;
using System;

namespace Emerald.Core
{
    internal sealed class CommandExecutionResultActor : ReceiveActor
    {
        private readonly Guid _commandId;
        private IActorRef _receiver;
        private CommandExecutionResult _commandExecutionResult;

        public CommandExecutionResultActor(Guid commandId)
        {
            _commandId = commandId;

            Receive<Guid>(msg => Handle(msg));
            Receive<CommandExecutionResult>(msg => Handle(msg));
        }

        private void Handle(Guid commandId)
        {
            if (_commandId != commandId) return;
            if (_commandExecutionResult != null) Sender.Tell(_commandExecutionResult);
            _receiver = Sender;
        }
        private void Handle(CommandExecutionResult result)
        {
            if (_commandId != result.CommandId) return;
            _receiver?.Tell(result);
            _commandExecutionResult = result;
        }
    }
}
using Akka.Routing;

namespace Emerald.Core
{
    public sealed class CommandEnvelope : IConsistentHashable
    {
        public CommandEnvelope(Command command, CommandProcessingLogBuilder commandProcessingLogBuilder)
        {
            Command = command;
            CommandProcessingLogBuilder = commandProcessingLogBuilder;
        }

        public Command Command { get; }
        public CommandProcessingLogBuilder CommandProcessingLogBuilder { get; }
        public object ConsistentHashKey => ((IConsistentHashable)Command).ConsistentHashKey;
    }
}
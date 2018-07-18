using Emerald.Abstractions;

namespace Emerald.AspNetCore.Infrastructure
{
    internal sealed class CommandExecutionStrategyFactory : ICommandExecutionStrategyFactory
    {
        public ICommandExecutionStrategy Create()
        {
            return new CommandExecutionStrategy();
        }
    }
}
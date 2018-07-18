using Emerald.Abstractions;

namespace Emerald.Common
{
    public sealed class DefaultCommandExecutionStrategyFactory : ICommandExecutionStrategyFactory
    {
        public ICommandExecutionStrategy Create()
        {
            return new DefaultCommandExecutionStrategy();
        }
    }
}
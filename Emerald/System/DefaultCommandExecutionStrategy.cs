using System;

namespace Emerald.System
{
    public sealed class DefaultCommandExecutionStrategy : CommandExecutionStrategy
    {
        protected override bool ShouldRetryOn(Exception exception)
        {
            return false;
        }
    }
}
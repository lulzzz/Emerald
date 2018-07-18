using Emerald.Abstractions;
using Emerald.Utils;
using System;
using System.Threading.Tasks;

namespace Emerald.AspNetCore.Infrastructure
{
    internal sealed class CommandExecutionStrategy : ICommandExecutionStrategy
    {
        public async Task Execute(Func<Task> handler)
        {
            await RetryHelper.ExecuteWithRetryForSqlException(handler);
        }
    }
}
using Emerald.Abstractions;
using System;
using System.Threading.Tasks;

namespace Emerald.Common
{
    public sealed class DefaultCommandExecutionStrategy : ICommandExecutionStrategy
    {
        public async Task Execute(Func<Task> handler)
        {
            await handler();
        }
    }
}
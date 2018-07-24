using Emerald.Utils;
using System;
using System.Threading.Tasks;

namespace Emerald.System
{
    public abstract class CommandExecutionStrategy
    {
        protected virtual TimeSpan MaxDelay { get; } = TimeSpan.FromSeconds(30);
        protected virtual int MaxRetryCount { get; } = 10;

        public Task Execute(Func<Task> operation)
        {
            return RetryHelper.Execute(operation, MaxDelay, MaxRetryCount, ShouldRetryOn);
        }
        public Task<TResult> Execute<TResult>(Func<Task<TResult>> operation)
        {
            return RetryHelper.Execute(operation, MaxDelay, MaxRetryCount, ShouldRetryOn);
        }

        protected abstract bool ShouldRetryOn(Exception exception);
    }
}
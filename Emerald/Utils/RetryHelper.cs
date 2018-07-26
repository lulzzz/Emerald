using System;
using System.Threading.Tasks;

namespace Emerald.Utils
{
    public static class RetryHelper
    {
        public static Task Execute(Func<Task> operation)
        {
            return Execute(operation, TimeSpan.FromSeconds(30), 10, ex => true);
        }
        public static async Task Execute(Func<Task> operation, TimeSpan maxDelay, int maxRetryCount, Func<Exception, bool> shouldRetryOn)
        {
            await Execute<object>(async () => { await operation(); return null; }, maxDelay, maxRetryCount, shouldRetryOn);
        }

        public static Task<TResult> Execute<TResult>(Func<Task<TResult>> operation)
        {
            return Execute(operation, TimeSpan.FromSeconds(30), 10, ex => true);
        }
        public static async Task<TResult> Execute<TResult>(Func<Task<TResult>> operation, TimeSpan maxDelay, int maxRetryCount, Func<Exception, bool> shouldRetryOn)
        {
            var retryNumber = 1;
            var delay = GetDelay(null, maxDelay);

            while (true)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    if (shouldRetryOn.Invoke(ex) == false) throw;
                    if (retryNumber >= maxRetryCount) throw;
                    retryNumber++;
                }

                await Task.Delay(delay);

                delay = GetDelay(delay, maxDelay);
            }
        }

        private static TimeSpan GetDelay(TimeSpan? previousDelay, TimeSpan maxDelay)
        {
            var delay = previousDelay.HasValue ? TimeSpan.FromMilliseconds(previousDelay.Value.TotalMilliseconds * 2) : TimeSpan.FromSeconds(1);
            delay = delay > maxDelay ? maxDelay : delay;
            return delay;
        }
    }
}
using System;
using System.Threading.Tasks;

namespace Emerald.Utils
{
    public static class RetryHelper
    {
        public static async Task Execute(Func<Task> action, int retryCount = 3, int delay = 3000)
        {
            await Execute<object>(async () => { await action(); return null; }, retryCount, delay);
        }
        public static async Task<TResult> Execute<TResult>(Func<Task<TResult>> action, int retryCount = 3, int delay = 3000)
        {
            var retry = 1;

            while (true)
            {
                try
                {
                    return await action();
                }
                catch
                {
                    if (retry > retryCount) throw;
                    retry++;
                }

                await Task.Delay(delay);
            }
        }
    }
}
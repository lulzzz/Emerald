using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Emerald.Utils
{
    public static class RetryHelper
    {
        public static async Task Execute(Func<Task> action, int retryCount = 5, int delay = 1000)
        {
            await Execute<object>(async () => { await action(); return null; }, retryCount, delay);
        }
        public static async Task<TResult> Execute<TResult>(Func<Task<TResult>> action, int retryCount = 5, int delay = 1000)
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

                await Task.Delay(TimeSpan.FromMilliseconds(delay));
            }
        }

        public static async Task ExecuteWithRetryForSqlException(Func<Task> action, int retryCount = 5, int delay = 1000)
        {
            var retry = 1;

            bool IsSqlException(Exception exception)
            {
                if (exception == null) return false;
                if (exception.GetType().IsAssignableFrom(typeof(SqlException))) return true;
                return IsSqlException(exception.InnerException);
            }

            while (true)
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception ex)
                {
                    if (retry > retryCount) throw;
                    if (!IsSqlException(ex)) throw;
                    retry++;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(delay));
            }
        }
    }
}
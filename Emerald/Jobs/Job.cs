using Emerald.Abstractions;
using Emerald.Core;
using System.Threading.Tasks;

namespace Emerald.Jobs
{
    public abstract class Job
    {
        protected internal CommandExecutor CommandExecutor { get; internal set; }
        protected internal IServiceScopeFactory ServiceScopeFactory { get; internal set; }
        protected internal ITransactionScopeFactory TransactionScopeFactory { get; internal set; }

        public abstract Task Execute();
    }
}
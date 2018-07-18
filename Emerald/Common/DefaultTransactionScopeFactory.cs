using Emerald.Abstractions;

namespace Emerald.Common
{
    public sealed class DefaultTransactionScopeFactory : ITransactionScopeFactory
    {
        public ITransactionScope Create(IServiceScope serviceScope)
        {
            return new DefaultTransactionScope();
        }
    }
}
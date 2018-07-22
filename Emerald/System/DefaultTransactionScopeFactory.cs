namespace Emerald.System
{
    public sealed class DefaultTransactionScopeFactory : ITransactionScopeFactory
    {
        public ITransactionScope Create(IServiceScope serviceScope)
        {
            return new DefaultTransactionScope();
        }
    }
}
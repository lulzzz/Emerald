namespace Emerald.System
{
    public interface ITransactionScopeFactory
    {
        ITransactionScope Create(IServiceScope serviceScope);
    }
}
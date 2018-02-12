namespace Emerald.Abstractions
{
    public interface ITransactionScopeFactory
    {
        ITransactionScope Create(IServiceScope serviceScope);
    }
}
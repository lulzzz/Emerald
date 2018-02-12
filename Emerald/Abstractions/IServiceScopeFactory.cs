namespace Emerald.Abstractions
{
    public interface IServiceScopeFactory
    {
        IServiceScope CreateScope();
    }
}
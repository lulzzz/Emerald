using Microsoft.Extensions.DependencyInjection;

namespace Emerald.Common
{
    public interface ITransactionScopeFactory
    {
        ITransactionScope Create(IServiceScope serviceScope);
    }
}
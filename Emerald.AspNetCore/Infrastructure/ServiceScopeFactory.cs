namespace Emerald.AspNetCore.Infrastructure
{
    internal sealed class ServiceScopeFactory : Abstractions.IServiceScopeFactory
    {
        private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _serviceScopeFactory;

        public ServiceScopeFactory(Microsoft.Extensions.DependencyInjection.IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Abstractions.IServiceScope CreateScope()
        {
            return new ServiceScope(_serviceScopeFactory.CreateScope());
        }
    }
}
namespace Emerald.AspNetCore.System
{
    internal sealed class ServiceScopeFactory : Emerald.System.IServiceScopeFactory
    {
        private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _serviceScopeFactory;

        public ServiceScopeFactory(Microsoft.Extensions.DependencyInjection.IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Emerald.System.IServiceScope Create()
        {
            return new ServiceScope(_serviceScopeFactory.CreateScope());
        }
    }
}
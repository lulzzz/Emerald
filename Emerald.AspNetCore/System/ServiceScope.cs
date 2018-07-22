using System;

namespace Emerald.AspNetCore.System
{
    internal sealed class ServiceScope : Emerald.System.IServiceScope
    {
        private readonly Microsoft.Extensions.DependencyInjection.IServiceScope _serviceScope;

        public ServiceScope(Microsoft.Extensions.DependencyInjection.IServiceScope serviceScope)
        {
            _serviceScope = serviceScope;
        }

        public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}
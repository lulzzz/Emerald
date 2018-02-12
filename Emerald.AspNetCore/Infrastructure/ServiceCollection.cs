using Microsoft.Extensions.DependencyInjection;

namespace Emerald.AspNetCore.Infrastructure
{
    internal sealed class ServiceCollection : Abstractions.IServiceCollection
    {
        private readonly IServiceCollection _serviceCollection;

        public ServiceCollection(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void AddSingleton<T>(T obj) where T : class
        {
            _serviceCollection.AddSingleton(obj);
        }

        public void AddScoped<T>() where T : class
        {
            _serviceCollection.AddScoped<T>();
        }
    }
}
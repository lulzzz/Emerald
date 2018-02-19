using Microsoft.Extensions.DependencyInjection;
using System;

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

        public void AddScoped(Type type)
        {
            _serviceCollection.AddScoped(type);
        }

        public void AddScoped<TService, TImplementation>() where TImplementation : class, TService where TService : class
        {
            _serviceCollection.AddScoped<TService, TImplementation>();
        }

        public IServiceProvider BuildServiceProvider()
        {
            return _serviceCollection.BuildServiceProvider();
        }
    }
}
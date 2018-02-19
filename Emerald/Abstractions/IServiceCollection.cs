using System;

namespace Emerald.Abstractions
{
    public interface IServiceCollection
    {
        void AddSingleton<T>(T obj) where T : class;
        void AddScoped(Type type);
        void AddScoped<TService, TImplementation>() where TImplementation : class, TService where TService : class;
        IServiceProvider BuildServiceProvider();
    }
}
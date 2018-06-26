using System;

namespace Emerald.Abstractions
{
    public interface IServiceCollection
    {
        void AddSingleton<T>(T obj) where T : class;
        void AddScoped(Type type);
        void AddScoped(Type serviceType, Type implementationType);
        IServiceProvider BuildServiceProvider();
    }
}
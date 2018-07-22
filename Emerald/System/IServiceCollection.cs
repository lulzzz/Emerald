using System;

namespace Emerald.System
{
    public interface IServiceCollection
    {
        void AddSingleton<T>(T obj) where T : class;
        void AddScoped(Type type);
        void AddScoped(Type serviceType, Type implementationType);
        void AddScoped<T>(Func<T> implementationFactory) where T : class;
        IServiceProvider BuildServiceProvider();
    }
}
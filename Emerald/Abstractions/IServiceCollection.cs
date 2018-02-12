namespace Emerald.Abstractions
{
    public interface IServiceCollection
    {
        void AddSingleton<T>(T obj) where T : class;
        void AddScoped<T>() where T : class;
    }
}
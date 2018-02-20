using Emerald.AspNetCore.Configuration;
using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;

namespace Emerald.AspNetCore.Common
{
    public sealed class EmeraldOptions
    {
        private readonly IEmeraldSystemBuilder _emeraldSystemBuilder;
        private readonly EnvironmentConfigurationSection _environment;

        internal EmeraldOptions(IEmeraldSystemBuilder emeraldSystemBuilder, EnvironmentConfigurationSection environment)
        {
            _emeraldSystemBuilder = emeraldSystemBuilder;
            _environment = environment;
        }

        public void AddCommandHandler<T>() where T : CommandHandler
        {
            _emeraldSystemBuilder.AddCommandHandler<T>();
        }

        public void AddJob<T>() where T : class, IJob
        {
            _emeraldSystemBuilder.AddJob<T>(_environment.Jobs[typeof(T).Name]);
        }

        public void UseQueue<T>() where T : EventListener
        {
            _emeraldSystemBuilder.UseQueue<T>(_environment.Queue.ConnectionString, _environment.Queue.Interval);
        }
    }
}
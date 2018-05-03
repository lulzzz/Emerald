using Emerald.AspNetCore.Configuration;
using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;
using System;

namespace Emerald.AspNetCore.Common
{
    public sealed class EmeraldOptions
    {
        private readonly IEmeraldSystemBuilder _emeraldSystemBuilder;
        private readonly ApplicationConfiguration _configuration;

        internal EmeraldOptions(IEmeraldSystemBuilder emeraldSystemBuilder, ApplicationConfiguration configuration)
        {
            _emeraldSystemBuilder = emeraldSystemBuilder;
            _configuration = configuration;
        }

        public void AddCommandHandler<T>() where T : CommandHandler
        {
            _emeraldSystemBuilder.AddCommandHandler<T>();
        }

        public void AddJob<T>() where T : class, IJob
        {
            _emeraldSystemBuilder.AddJob<T>(_configuration.Environment.Jobs[typeof(T).Name]);
        }

        public void UseQueue(Action<QueueListenerConfig> configureQueueListener)
        {
            var connectionString = _configuration.Environment.Queue.ConnectionString;
            var interval = _configuration.Environment.Queue.Interval;
            var listen = _configuration.Environment.Queue.Listen;
            _emeraldSystemBuilder.UseQueue(connectionString, interval, listen, configureQueueListener);
        }
    }
}
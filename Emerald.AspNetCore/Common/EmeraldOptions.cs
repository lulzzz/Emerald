using Emerald.Core;
using Emerald.Jobs;
using System;

namespace Emerald.AspNetCore.Common
{
    public sealed class EmeraldOptions
    {
        private readonly EmeraldSystemBuilder _emeraldSystemBuilder;

        internal EmeraldOptions(EmeraldSystemBuilder emeraldSystemBuilder)
        {
            _emeraldSystemBuilder = emeraldSystemBuilder;
        }

        public void AddCommandHandler<T>() where T : CommandHandler
        {
            _emeraldSystemBuilder.AddCommandHandler<T>();
        }

        public void AddJob<T>(string cron) where T : class, IJob
        {
            _emeraldSystemBuilder.AddJob<T>(cron);
        }

        public void UseQueue(string connectionString, Action<QueueOptions> configure)
        {
            _emeraldSystemBuilder.UseQueue(connectionString, cfg => configure(new QueueOptions(cfg)));
        }
    }
}
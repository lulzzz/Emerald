using Emerald.AspNetCore.Configuration;
using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;
using Emerald.System;
using Emerald.Utils;
using System;
using System.Linq;

namespace Emerald.AspNetCore.System
{
    public sealed class EmeraldOptions
    {
        private readonly EmeraldSystemBuilderFirstStepConfig _emeraldSystemBuilderConfig;
        private readonly IApplicationConfiguration _configuration;

        internal bool MemoryCacheEnabled { get; private set; }
        internal bool SwaggerEnabled { get; private set; }
        internal string SwaggerEndpoint { get; private set; }
        internal string SwaggerApiName { get; private set; }
        internal string SwaggerApiVersion { get; private set; }

        internal EmeraldOptions(EmeraldSystemBuilderFirstStepConfig emeraldSystemBuilderConfig, IApplicationConfiguration configuration)
        {
            _emeraldSystemBuilderConfig = emeraldSystemBuilderConfig;
            _configuration = configuration;
        }

        public void AddCommandHandler<T>() where T : CommandHandler
        {
            _emeraldSystemBuilderConfig.AddCommandHandler<T>();
        }
        public void AddJob<T>() where T : IJob
        {
            var jobConig = _configuration.Environment.Jobs.Single(c => c.Name == typeof(T).Name);
            _emeraldSystemBuilderConfig.AddJob<T>(jobConig.Enabled, jobConig.Expression);
        }
        public void UseQueue(Action<QueueConfig> configure)
        {
            var connectionString = _configuration.Environment.Queue.ConnectionString;
            var interval = _configuration.Environment.Queue.Interval;
            var listen = _configuration.Environment.Queue.Listen;
            _emeraldSystemBuilderConfig.UseQueue(connectionString, interval, listen, configure);
        }

        public void SetTransactionScopeFactory<T>() where T : class, ITransactionScopeFactory
        {
            _emeraldSystemBuilderConfig.SetTransactionScopeFactory<T>();
        }
        public void SetCommandExecutionStrategy<T>() where T : CommandExecutionStrategy
        {
            _emeraldSystemBuilderConfig.SetCommandExecutionStrategy<T>();
        }

        public void UseMemoryCache()
        {
            MemoryCacheEnabled = true;
        }
        public void UseSwagger(string endpoint, string name, string version)
        {
            SwaggerEnabled = true;
            SwaggerEndpoint = endpoint;
            SwaggerApiName = name;
            SwaggerApiVersion = ValidationHelper.IsNullOrEmptyOrWhiteSpace(version) ? "v1" : version;
        }
    }
}
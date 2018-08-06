using Emerald.AspNetCore.Configuration;
using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;
using Emerald.System;
using System;
using System.Linq;

namespace Emerald.AspNetCore.System
{
    public sealed class EmeraldOptions
    {
        private readonly EmeraldSystemBuilderFirstStepConfig _emeraldSystemBuilderConfig;
        private readonly IApplicationConfiguration _configuration;

        internal EmeraldOptions(EmeraldSystemBuilderFirstStepConfig emeraldSystemBuilderConfig, IApplicationConfiguration configuration)
        {
            _emeraldSystemBuilderConfig = emeraldSystemBuilderConfig;
            _configuration = configuration;
        }

        public void AddCommandHandler<T>() where T : CommandHandler
        {
            _emeraldSystemBuilderConfig.AddCommandHandler<T>();
        }
        public void UseJobs(Action<JobsConfig> configure)
        {
            _emeraldSystemBuilderConfig.UseJobs(cfg => configure(new JobsConfig(cfg, _configuration)));
        }
        public void UseQueue(Action<QueueConfig> configure)
        {
            var connectionString = _configuration.Environment.Queue.ConnectionString;
            var delay = _configuration.Environment.Queue.Delay;
            var interval = _configuration.Environment.Queue.Interval;
            var listen = _configuration.Environment.Queue.Listen;
            _emeraldSystemBuilderConfig.UseQueue(connectionString, delay, interval, listen, configure);
        }

        public void SetTransactionScopeFactory<T>() where T : class, ITransactionScopeFactory
        {
            _emeraldSystemBuilderConfig.SetTransactionScopeFactory<T>();
        }
        public void SetCommandExecutionStrategy<T>() where T : CommandExecutionStrategy
        {
            _emeraldSystemBuilderConfig.SetCommandExecutionStrategy<T>();
        }
    }

    public sealed class JobsConfig
    {
        private readonly Jobs.JobsConfig _jobsConfig;
        private readonly IApplicationConfiguration _configuration;

        internal JobsConfig(Jobs.JobsConfig jobsConfig, IApplicationConfiguration configuration)
        {
            _jobsConfig = jobsConfig;
            _configuration = configuration;
        }

        public JobsConfig AddJob<T>() where T : IJob
        {
            var jobConig = _configuration.Environment.Jobs.Single(c => c.Name == typeof(T).Name);
            _jobsConfig.AddJob<T>(jobConig.Enabled, jobConig.Expression);
            return this;
        }
    }
}
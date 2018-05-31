using Emerald.Application;
using Emerald.AspNetCore.Configuration;
using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;
using System;
using System.Linq;

namespace Emerald.AspNetCore.Common
{
    public sealed class EmeraldOptions
    {
        private readonly IEmeraldSystemBuilder _emeraldSystemBuilder;
        private readonly ApplicationConfiguration _configuration;

        internal bool AuthenticationEnabled { get; private set; }
        internal Type AuthenticationServiceType { get; private set; }
        internal bool MemoryCacheEnabled { get; private set; }
        internal bool SwaggerEnabled { get; private set; }
        internal string SwaggerEndpoint { get; private set; }
        internal string SwaggerApiName { get; private set; }
        internal string SwaggerApiVersion { get; private set; }

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
            var jobConig = _configuration.Environment.Jobs.Single(c => c.Name == typeof(T).Name);
            if (jobConig.Enabled == false) return;
            _emeraldSystemBuilder.AddJob<T>(jobConig.CronTab);
        }
        public QueueConfig UseQueue()
        {
            var connectionString = _configuration.Environment.Queue.ConnectionString;
            var interval = _configuration.Environment.Queue.Interval;
            var listen = _configuration.Environment.Queue.Listen;
            return _emeraldSystemBuilder.UseQueue(connectionString, interval, listen);
        }

        public void UseAuthentication<T>()
        {
            AuthenticationEnabled = true;
            AuthenticationServiceType = typeof(T);
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
using Microsoft.Extensions.Configuration;
using System;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class QueueConfigurationSection
    {
        internal QueueConfigurationSection(IConfiguration configuration)
        {
            ConnectionString = configuration.GetSection("environment:queue").GetValue<string>("connectionString");
            Delay = TimeSpan.FromMilliseconds(configuration.GetSection("environment:queue").GetValue<long>("delay"));
            Interval = TimeSpan.FromMilliseconds(configuration.GetSection("environment:queue").GetValue<long>("interval"));
            Listen = configuration.GetSection("environment:queue").GetValue<bool>("listen");
        }

        public string ConnectionString { get; }
        public TimeSpan Delay { get; }
        public TimeSpan Interval { get; }
        public bool Listen { get; }
    }
}
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class EnvironmentConfigurationSection
    {
        internal EnvironmentConfigurationSection(IConfiguration configuration)
        {
            ApplicationDb = new ApplicationDbConfigurationSection(configuration);
            ApplicationInsights = new ApplicationInsightsConfigurationSection(configuration);
            ApplicationName = configuration.GetSection("environment").GetValue<string>("applicationName");
            Development = new DevelopmentConfigurationSection(configuration);
            Logging = new LoggingConfigurationSection(configuration);
            Name = configuration.GetSection("environment").GetValue<string>("name");
            Jobs = new Dictionary<string, string>();
            Queue = new QueueConfigurationSection(configuration);

            foreach (var item in configuration.GetSection("environment:jobs").GetChildren())
            {
                if (item.GetValue<bool>("enabled")) Jobs.Add(item.GetValue<string>("name"), item.GetValue<string>("crontab"));
            }
        }

        public ApplicationDbConfigurationSection ApplicationDb { get; }
        public ApplicationInsightsConfigurationSection ApplicationInsights { get; }
        public string ApplicationName { get; }
        public DevelopmentConfigurationSection Development { get; }
        public LoggingConfigurationSection Logging { get; }
        public string Name { get; }
        public Dictionary<string, string> Jobs { get; }
        public QueueConfigurationSection Queue { get; }
    }
}
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class EnvironmentConfigurationSection
    {
        internal EnvironmentConfigurationSection(IConfiguration configuration)
        {
            ApplicationDb = new ApplicationDbConfigurationSection(configuration);
            ApplicationName = configuration.GetSection("environment").GetValue<string>("applicationName");
            Host = configuration.GetSection("environment").GetValue<string>("host");
            Name = configuration.GetSection("environment").GetValue<string>("name");
            Jobs = new Dictionary<string, string>();
            Queue = new QueueConfigurationSection(configuration);

            foreach (var item in configuration.GetSection("environment:jobs").GetChildren())
            {
                if (item.GetValue<bool>("enabled")) Jobs.Add(item.GetValue<string>("name"), item.GetValue<string>("crontab"));
            }
        }

        public ApplicationDbConfigurationSection ApplicationDb { get; }
        public string ApplicationName { get; }
        public string Name { get; }
        public string Host { get; }
        public Dictionary<string, string> Jobs { get; }
        public QueueConfigurationSection Queue { get; }
    }
}
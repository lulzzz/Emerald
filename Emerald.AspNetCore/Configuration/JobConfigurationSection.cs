using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class JobConfigurationSection
    {
        public JobConfigurationSection(string cronTab, bool enabled, string name)
        {
            CronTab = cronTab;
            Enabled = enabled;
            Name = name;
        }

        public string CronTab { get; }
        public bool Enabled { get; }
        public string Name { get; }

        internal static JobConfigurationSection[] Create(IConfiguration configuration)
        {
            var jobConfigurationSectionList = new List<JobConfigurationSection>();

            foreach (var item in configuration.GetSection("environment:jobs").GetChildren())
            {
                var cronTab = item.GetValue<string>("crontab");
                var enabled = item.GetValue<bool>("enabled");
                var name = item.GetValue<string>("name");
                jobConfigurationSectionList.Add(new JobConfigurationSection(cronTab, enabled, name));
            }

            return jobConfigurationSectionList.ToArray();
        }
    }
}
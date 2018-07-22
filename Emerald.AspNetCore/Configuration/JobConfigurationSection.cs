using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class JobConfigurationSection
    {
        public JobConfigurationSection(bool enabled, string expression, string name)
        {
            Enabled = enabled;
            Expression = expression;
            Name = name;
        }

        public bool Enabled { get; }
        public string Expression { get; }
        public string Name { get; }

        internal static JobConfigurationSection[] Create(IConfiguration configuration)
        {
            var jobConfigurationSectionList = new List<JobConfigurationSection>();

            foreach (var item in configuration.GetSection("environment:jobs").GetChildren())
            {
                var enabled = item.GetValue<bool>("enabled");
                var expression = item.GetValue<string>("expression");
                var name = item.GetValue<string>("name");
                jobConfigurationSectionList.Add(new JobConfigurationSection(enabled, expression, name));
            }

            return jobConfigurationSectionList.ToArray();
        }
    }
}
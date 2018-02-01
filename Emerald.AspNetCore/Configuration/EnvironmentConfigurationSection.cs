using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class EnvironmentConfigurationSection
    {
        private readonly IConfigurationSection _configurationSection;

        internal EnvironmentConfigurationSection(IConfiguration configuration)
        {
            _configurationSection = configuration.GetSection("environment");
            ApplicationDb = new ApplicationDbConfigurationSection(configuration);
            QueueDb = new QueueDbConfigurationSection(configuration);
        }

        public string Name => _configurationSection.GetValue<string>("name");
        public string ApplicationName => _configurationSection.GetValue<string>("applicationName");
        public string Host => _configurationSection.GetValue<string>("host");
        public ApplicationDbConfigurationSection ApplicationDb { get; }
        public QueueDbConfigurationSection QueueDb { get; }
    }
}
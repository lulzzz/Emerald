using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class QueueDbConfigurationSection
    {
        private readonly IConfigurationSection _configurationSection;

        internal QueueDbConfigurationSection(IConfiguration configuration)
        {
            _configurationSection = configuration.GetSection("environment:queueDb");
        }

        public string ConnectionString => _configurationSection.GetValue<string>("connectionString");
    }
}
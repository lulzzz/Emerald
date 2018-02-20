using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class LoggingConfigurationSection
    {
        internal LoggingConfigurationSection(IConfiguration configuration)
        {
            Console = new ConsoleLoggingConfigurationSection(configuration);
            ElasticSearch = new ElasticSearchLoggingConfigurationSection(configuration);
        }

        public ConsoleLoggingConfigurationSection Console { get; }
        public ElasticSearchLoggingConfigurationSection ElasticSearch { get; }
    }
}
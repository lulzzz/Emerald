using Microsoft.Extensions.Configuration;
using Serilog.Events;
using System;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class LoggingConfigurationSection
    {
        internal LoggingConfigurationSection(IConfiguration configuration)
        {
            Console = new ConsoleLoggingConfigurationSection(configuration);
            ElasticSearch = new ElasticSearchLoggingConfigurationSection(configuration);
            Level = Enum.Parse<LogEventLevel>(configuration.GetSection("environment:logging").GetValue<string>("level"));
        }

        public ConsoleLoggingConfigurationSection Console { get; }
        public ElasticSearchLoggingConfigurationSection ElasticSearch { get; }
        public LogEventLevel Level { get; }
    }
}
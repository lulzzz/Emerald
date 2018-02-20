using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class ElasticSearchLoggingConfigurationSection
    {
        internal ElasticSearchLoggingConfigurationSection(IConfiguration configuration)
        {
            Enabled = configuration.GetSection("environment:logging:elasticSearch").GetValue<bool>("enabled");
            IndexFormat = configuration.GetSection("environment:logging:elasticSearch").GetValue<string>("indexFormat");
            NodeUri = configuration.GetSection("environment:logging:elasticSearch").GetValue<string>("nodeUri");
        }

        public bool Enabled { get; }
        public string IndexFormat { get; }
        public string NodeUri { get; }
    }
}
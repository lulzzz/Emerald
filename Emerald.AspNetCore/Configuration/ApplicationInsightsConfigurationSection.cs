using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class ApplicationInsightsConfigurationSection
    {
        internal ApplicationInsightsConfigurationSection(IConfiguration configuration)
        {
            Enabled = configuration.GetSection("environment:applicationInsights").GetValue<bool>("enabled");
            Key = configuration.GetSection("environment:applicationInsights").GetValue<string>("key");
        }

        public bool Enabled { get; }
        public string Key { get; }
    }
}
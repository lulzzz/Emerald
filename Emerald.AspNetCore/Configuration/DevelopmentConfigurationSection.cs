using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class DevelopmentConfigurationSection
    {
        internal DevelopmentConfigurationSection(IConfiguration configuration)
        {
            Host = configuration.GetSection("environment:development").GetValue<string>("host");
        }
        public string Host { get; }
    }
}
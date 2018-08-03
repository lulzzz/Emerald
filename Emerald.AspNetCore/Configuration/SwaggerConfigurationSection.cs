using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public class SwaggerConfigurationSection
    {
        internal SwaggerConfigurationSection(IConfiguration configuration)
        {
            Enabled = configuration.GetSection("environment:swagger").GetValue<bool>("enabled");
            Endpoint = configuration.GetSection("environment:swagger").GetValue<string>("endpoint");
            ApiName = configuration.GetSection("environment:swagger").GetValue<string>("apiName");
            ApiVersion = configuration.GetSection("environment:swagger").GetValue<string>("apiVersion");
        }

        public bool Enabled { get; }
        public string Endpoint { get; }
        public string ApiName { get; }
        public string ApiVersion { get; }
    }
}
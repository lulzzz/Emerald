using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class JwtConfigurationSection
    {
        internal JwtConfigurationSection(IConfiguration configuration)
        {
            Key = configuration.GetSection("environment:jwt").GetValue<string>("key");
        }

        public string Key { get; }
    }
}
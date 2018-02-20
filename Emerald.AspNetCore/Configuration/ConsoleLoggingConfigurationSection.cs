using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class ConsoleLoggingConfigurationSection
    {
        internal ConsoleLoggingConfigurationSection(IConfiguration configuration)
        {
            Enabled = configuration.GetSection("environment:logging:console").GetValue<bool>("enabled");
        }

        public bool Enabled { get; }
    }
}
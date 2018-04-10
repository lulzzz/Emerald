using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class ApplicationConfiguration : IApplicationConfiguration
    {
        internal ApplicationConfiguration(IConfiguration configuration)
        {
            Environment = new EnvironmentConfigurationSection(configuration);
        }

        public EnvironmentConfigurationSection Environment { get; }
    }

    public interface IApplicationConfiguration
    {
        EnvironmentConfigurationSection Environment { get; }
    }
}
using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class ApplicationConfiguration : IApplicationConfiguration
    {
        internal ApplicationConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
            Environment = new EnvironmentConfigurationSection(configuration);
        }

        public IConfiguration Configuration { get; }
        public EnvironmentConfigurationSection Environment { get; }
    }

    public interface IApplicationConfiguration
    {
        IConfiguration Configuration { get; }
        EnvironmentConfigurationSection Environment { get; }
    }
}
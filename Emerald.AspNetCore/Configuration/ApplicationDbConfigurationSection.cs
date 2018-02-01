using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class ApplicationDbConfigurationSection
    {
        private readonly IConfigurationSection _configurationSection;

        internal ApplicationDbConfigurationSection(IConfiguration configuration)
        {
            _configurationSection = configuration.GetSection("environment:applicationDb");
        }

        public string ConnectionString => _configurationSection.GetValue<string>("connectionString");
        public bool MigrateDatabaseToLatestVersion => _configurationSection.GetValue<bool>("migrateDatabaseToLatestVersion");
        public bool ExecuteSeed => _configurationSection.GetValue<bool>("executeSeed");
    }
}
using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class ApplicationDbConfigurationSection
    {
        internal ApplicationDbConfigurationSection(IConfiguration configuration)
        {
            ConnectionString = configuration.GetSection("environment:applicationDb").GetValue<string>("connectionString");
            MigrateDatabaseToLatestVersion = configuration.GetSection("environment:applicationDb").GetValue<bool>("migrateDatabaseToLatestVersion");
            ExecuteSeed = configuration.GetSection("environment:applicationDb").GetValue<bool>("executeSeed");
        }

        public string ConnectionString { get; }
        public bool MigrateDatabaseToLatestVersion { get; }
        public bool ExecuteSeed { get; }
    }
}
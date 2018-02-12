using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class QueueConfigurationSection
    {
        internal QueueConfigurationSection(IConfiguration configuration)
        {
            ConnectionString = configuration.GetSection("environment:queue").GetValue<string>("connectionString");
            Interval = configuration.GetSection("environment:queue").GetValue<long>("interval");
        }

        public string ConnectionString { get; }
        public long Interval { get; }
    }
}
﻿using Microsoft.Extensions.Configuration;

namespace Emerald.AspNetCore.Configuration
{
    public sealed class EnvironmentConfigurationSection
    {
        internal EnvironmentConfigurationSection(IConfiguration configuration)
        {
            ApplicationDb = new ApplicationDbConfigurationSection(configuration);
            ApplicationInsights = new ApplicationInsightsConfigurationSection(configuration);
            ApplicationName = configuration.GetSection("environment").GetValue<string>("applicationName");
            Development = new DevelopmentConfigurationSection(configuration);
            Logging = new LoggingConfigurationSection(configuration);
            Name = configuration.GetSection("environment").GetValue<string>("name");
            Jobs = JobConfigurationSection.Create(configuration);
            Queue = new QueueConfigurationSection(configuration);
            Swagger = new SwaggerConfigurationSection(configuration);
        }

        public ApplicationDbConfigurationSection ApplicationDb { get; }
        public ApplicationInsightsConfigurationSection ApplicationInsights { get; }
        public string ApplicationName { get; }
        public DevelopmentConfigurationSection Development { get; }
        public LoggingConfigurationSection Logging { get; }
        public string Name { get; }
        public JobConfigurationSection[] Jobs { get; }
        public QueueConfigurationSection Queue { get; }
        public SwaggerConfigurationSection Swagger { get; }
    }
}
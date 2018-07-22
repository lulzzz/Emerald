using Emerald.AspNetCore.ApplicationInsights;
using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.Filters;
using Emerald.AspNetCore.System;
using Emerald.System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using IServiceCollection = Microsoft.Extensions.DependencyInjection.IServiceCollection;

namespace Emerald.AspNetCore.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEmerald<TServiceScopeFactory>(this IServiceCollection services, IApplicationConfiguration configuration, Action<EmeraldOptions> options) where TServiceScopeFactory : class, Emerald.System.IServiceScopeFactory
        {
            services.AddSingleton(configuration);

            var serviceCollection = new System.ServiceCollection(services);
            var applicationName = configuration.Environment.ApplicationName;
            var emeraldSystemBuilderConfig = EmeraldSystemBuilder.Create<TServiceScopeFactory>(applicationName, serviceCollection);
            var emeraldOptions = new EmeraldOptions(emeraldSystemBuilderConfig, configuration);
            options(emeraldOptions);

            Registry.EmeraldOptions = emeraldOptions;
            Registry.EmeraldSystem = emeraldSystemBuilderConfig.RegisterDependencies().Build();

            if (configuration.Environment.ApplicationInsights.Enabled)
            {
                services.AddSingleton<ITelemetryInitializer>(new TelemetryInitializer(configuration.Environment.ApplicationName));
                services.AddApplicationInsightsTelemetry(configuration.Environment.ApplicationInsights.Key);
            }

            if (emeraldOptions.MemoryCacheEnabled) services.AddMemoryCache();

            services.AddMvc(opt => opt.Filters.Add<LoggerActionFilter>());

            if (emeraldOptions.SwaggerEnabled)
            {
                var title = emeraldOptions.SwaggerApiName;
                var version = emeraldOptions.SwaggerApiVersion;
                services.AddSwaggerGen(c => { c.SwaggerDoc(version, new Info { Title = title, Version = version }); });
            }

            return services;
        }
    }
}
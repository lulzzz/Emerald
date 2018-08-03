using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.System;
using Emerald.System;
using Microsoft.Extensions.DependencyInjection;
using System;
using IServiceCollection = Microsoft.Extensions.DependencyInjection.IServiceCollection;

namespace Emerald.AspNetCore.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEmerald<TServiceScopeFactory>(this IServiceCollection services, IApplicationConfiguration configuration, Action<EmeraldOptions> options) where TServiceScopeFactory : class, Emerald.System.IServiceScopeFactory
        {
            var serviceCollection = new System.ServiceCollection(services);
            var applicationName = configuration.Environment.ApplicationName;
            var emeraldSystemBuilderConfig = EmeraldSystemBuilder.Create<TServiceScopeFactory>(applicationName, serviceCollection);
            var emeraldOptions = new EmeraldOptions(emeraldSystemBuilderConfig, configuration);
            options(emeraldOptions);
            Registry.EmeraldSystem = emeraldSystemBuilderConfig.RegisterDependencies().Build(services.BuildServiceProvider());
            return services;
        }
    }
}
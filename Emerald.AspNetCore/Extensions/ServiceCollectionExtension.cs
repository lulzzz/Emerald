using Emerald.AspNetCore.Common;
using Emerald.AspNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Emerald.AspNetCore.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEmerald(this IServiceCollection serviceCollection, EnvironmentConfigurationSection environment, Action<EmeraldOptions> options)
        {
            var builder = new EmeraldSystemBuilder(environment.ApplicationName, new Infrastructure.ServiceCollection(serviceCollection));
            options(new EmeraldOptions(environment, builder));
            Registry.EmeraldSystemBuilder = builder;
            return serviceCollection;
        }
    }
}
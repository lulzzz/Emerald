using Emerald.Abstractions;
using Emerald.AspNetCore.Common;
using Emerald.AspNetCore.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Emerald.AspNetCore.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddEmerald<TServiceScopeFactory, TTransactionScopeFactory>(
            this Microsoft.Extensions.DependencyInjection.IServiceCollection services,
            Action<EmeraldOptions> options) where TServiceScopeFactory : class, Abstractions.IServiceScopeFactory where TTransactionScopeFactory : class, ITransactionScopeFactory
        {
            var serviceProvider = services.BuildServiceProvider();
            var environment = new EnvironmentConfigurationSection(serviceProvider.GetService<IConfiguration>());
            var emeraldSystemBuilder = new EmeraldSystemBuilder<TServiceScopeFactory, TTransactionScopeFactory>(environment.ApplicationName, new Infrastructure.ServiceCollection(services));
            options(new EmeraldOptions(emeraldSystemBuilder, environment));
            Registry.EmeraldSystem = emeraldSystemBuilder.Build();
            return services;
        }
    }
}
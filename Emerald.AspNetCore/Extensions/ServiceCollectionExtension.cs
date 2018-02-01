using Emerald.AspNetCore.Common;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Emerald.AspNetCore.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEmerald(this IServiceCollection serviceCollection, string applicationName, Action<EmeraldOptions> options)
        {
            var builder = new EmeraldSystemBuilder(applicationName, serviceCollection);
            options(new EmeraldOptions(builder));
            Registry.EmeraldSystemBuilder = builder;
            return serviceCollection;
        }
    }
}
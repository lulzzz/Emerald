﻿using Emerald.AspNetCore.Common;
using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.Infrastructure;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Text;

namespace Emerald.AspNetCore.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEmerald<TCommandExecutionFactory, TServiceScopeFactory, TTransactionScopeFactory>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<EmeraldOptions> options)
            where TCommandExecutionFactory : class, Abstractions.ICommandExecutionStrategyFactory
            where TServiceScopeFactory : class, Abstractions.IServiceScopeFactory
            where TTransactionScopeFactory : class, Abstractions.ITransactionScopeFactory
        {
            var applicationConfiguration = new ApplicationConfiguration(configuration);
            services.AddSingleton<IApplicationConfiguration>(applicationConfiguration);

            var serviceCollection = new Infrastructure.ServiceCollection(services);
            var applicationName = applicationConfiguration.Environment.ApplicationName;
            var emeraldSystemBuilderConfig = EmeraldSystemBuilder.Create<TServiceScopeFactory>(applicationName, serviceCollection);
            emeraldSystemBuilderConfig.SetCommandExecutionStrategyFactory<TCommandExecutionFactory>();
            emeraldSystemBuilderConfig.SetTransactionScopeFactory<TTransactionScopeFactory>();

            var emeraldOptions = new EmeraldOptions(emeraldSystemBuilderConfig, applicationConfiguration);
            options(emeraldOptions);

            Registry.EmeraldOptions = emeraldOptions;
            Registry.EmeraldSystem = emeraldSystemBuilderConfig.RegisterDependencies().Build();

            if (applicationConfiguration.Environment.ApplicationInsights.Enabled)
            {
                services.AddSingleton<ITelemetryInitializer>(new TelemetryInitializer(applicationConfiguration.Environment.ApplicationName));
                services.AddApplicationInsightsTelemetry(applicationConfiguration.Environment.ApplicationInsights.Key);
            }

            if (emeraldOptions.AuthenticationEnabled)
            {
                var symmetricSecurityKeyFilePath = applicationConfiguration.Environment.Jwt.Key;
                var symmetricSecurityKeyFileContent = File.ReadAllText(symmetricSecurityKeyFilePath);
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricSecurityKeyFileContent));

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt => opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = symmetricSecurityKey
                });
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
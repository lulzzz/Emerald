using Emerald.Abstractions;
using Emerald.AspNetCore.Common;
using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddEmerald<TDbContext, TServiceScopeFactory, TTransactionScopeFactory>(
            this Microsoft.Extensions.DependencyInjection.IServiceCollection services,
            IConfiguration configuration,
            Action<EmeraldOptions> options)
                where TDbContext : DbContext
                where TServiceScopeFactory : class, Abstractions.IServiceScopeFactory
                where TTransactionScopeFactory : class, ITransactionScopeFactory
        {
            var applicationConfiguration = new ApplicationConfiguration(configuration);
            services.AddSingleton<IApplicationConfiguration>(applicationConfiguration);

            var serviceCollection = new Infrastructure.ServiceCollection(services);
            var applicationName = applicationConfiguration.Environment.ApplicationName;
            var emeraldSystemBuilder = new EmeraldSystemBuilder<TServiceScopeFactory, TTransactionScopeFactory>(applicationName, serviceCollection);
            var emeraldOptions = new EmeraldOptions(emeraldSystemBuilder, applicationConfiguration);
            options(emeraldOptions);

            Registry.EmeraldOptions = emeraldOptions;
            Registry.EmeraldSystem = emeraldSystemBuilder.Build();

            services.AddDbContext<TDbContext>(opt => opt.UseSqlServer(applicationConfiguration.Environment.ApplicationDb.ConnectionString));

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
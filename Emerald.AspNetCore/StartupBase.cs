﻿using Emerald.AspNetCore.Common;
using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.Extensions;
using Emerald.AspNetCore.Filters;
using Emerald.AspNetCore.Transaction;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Emerald.AspNetCore
{
    public abstract class StartupBase<TDbContext> where TDbContext : DbContext
    {
        private readonly EnvironmentConfigurationSection _environmentConfiguration;

        protected StartupBase(IConfiguration configuration)
        {
            Configuration = configuration;
            _environmentConfiguration = new EnvironmentConfigurationSection(configuration);
        }

        protected IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TDbContext>(options => options.UseSqlServer(_environmentConfiguration.ApplicationDb.ConnectionString));
            ConfigureDependencies(services);
            services.AddEmerald(_environmentConfiguration.ApplicationName, ConfigureEmerald);
            services.AddMvc(options => options.Filters.Add<EmeraldActionFilter>());
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info { Title = $"{_environmentConfiguration.ApplicationName} api", Version = "v1" }); });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseEmerald(new TransactionScopeFactory<TDbContext>());
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{_environmentConfiguration.ApplicationName} api v1"); });
        }

        protected abstract void ConfigureDependencies(IServiceCollection serviceCollection);
        protected abstract void ConfigureEmerald(EmeraldOptions options);
    }
}
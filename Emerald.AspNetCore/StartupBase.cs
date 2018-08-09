using Emerald.AspNetCore.ApplicationInsights;
using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.EntityFrameworkCore;
using Emerald.AspNetCore.Extensions;
using Emerald.AspNetCore.Filters;
using Emerald.AspNetCore.System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;

namespace Emerald.AspNetCore
{
    public abstract class StartupBase<TDbContext> where TDbContext : DbContext
    {
        protected StartupBase(IConfiguration configuration)
        {
            ApplicationConfiguration = new ApplicationConfiguration(configuration);
        }

        protected IApplicationConfiguration ApplicationConfiguration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(ApplicationConfiguration);
            ConfigureDbContext(services);

            ConfigureDependencies(services);
            ConfigureApplicationServices(services);

            services.AddEmerald<ServiceScopeFactory>(ApplicationConfiguration, ConfigureEmerald);
            services.AddMvc(opt => opt.Filters.Add<LoggerActionFilter>());

            if (ApplicationConfiguration.Environment.ApplicationInsights.Enabled)
            {
                services.AddSingleton<ITelemetryInitializer>(new TelemetryInitializer(ApplicationConfiguration.Environment.ApplicationName));
                services.AddApplicationInsightsTelemetry(ApplicationConfiguration.Environment.ApplicationInsights.Key);
            }

            if (ApplicationConfiguration.Environment.Swagger.Enabled)
            {
                var title = ApplicationConfiguration.Environment.Swagger.ApiName;
                var version = ApplicationConfiguration.Environment.Swagger.ApiVersion;
                services.AddSwaggerGen(c => { c.SwaggerDoc(version, new Info { Title = title, Version = version }); });
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            env.ApplicationName = ApplicationConfiguration.Environment.ApplicationName;
            env.EnvironmentName = ApplicationConfiguration.Environment.Name;

            app.UseEmerald();
            app.UseMvc();

            if (ApplicationConfiguration.Environment.Swagger.Enabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI(opt => { opt.SwaggerEndpoint(ApplicationConfiguration.Environment.Swagger.Endpoint, ApplicationConfiguration.Environment.Swagger.ApiName); });
            }
        }

        protected abstract void ConfigureDependencies(IServiceCollection services);
        protected virtual void ConfigureApplicationServices(IServiceCollection services)
        {
        }

        protected virtual void ConfigureDbContext(IServiceCollection services)
        {
            services.AddDbContextPool<TDbContext>(opt => opt.UseSqlServer(ApplicationConfiguration.Environment.ApplicationDb.ConnectionString, opt2 => opt2.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null)));
        }
        protected abstract void ConfigureEmerald(EmeraldOptions options);
        protected TDbContext CreateDbContext()
        {
            return DbContextFactory.Create<TDbContext>(ApplicationConfiguration.Environment.ApplicationDb.ConnectionString);
        }
    }
}
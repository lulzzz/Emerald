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

namespace Emerald.AspNetCore
{
    public abstract class StartupBase<TDbContext> where TDbContext : DbContext
    {
        protected StartupBase(IConfiguration configuration)
        {
            Configuration = new ApplicationConfiguration(configuration);
        }

        protected IApplicationConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<TDbContext>(opt => opt.UseSqlServer(Configuration.Environment.ApplicationDb.ConnectionString));
            services.AddSingleton(Configuration);

            ConfigureDependencies(services);
            ConfigureApplicationServices(services);

            services.AddEmerald<ServiceScopeFactory>(Configuration, ConfigureEmerald);
            services.AddMvc(opt => opt.Filters.Add<LoggerActionFilter>());

            if (Configuration.Environment.ApplicationInsights.Enabled)
            {
                services.AddSingleton<ITelemetryInitializer>(new TelemetryInitializer(Configuration.Environment.ApplicationName));
                services.AddApplicationInsightsTelemetry(Configuration.Environment.ApplicationInsights.Key);
            }

            if (Configuration.Environment.Swagger.Enabled)
            {
                var title = Configuration.Environment.Swagger.ApiName;
                var version = Configuration.Environment.Swagger.ApiVersion;
                services.AddSwaggerGen(c => { c.SwaggerDoc(version, new Info { Title = title, Version = version }); });
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            env.ApplicationName = Configuration.Environment.ApplicationName;
            env.EnvironmentName = Configuration.Environment.Name;

            app.UseEmerald();
            app.UseMvc();

            if (Configuration.Environment.Swagger.Enabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI(opt => { opt.SwaggerEndpoint(Configuration.Environment.Swagger.Endpoint, Configuration.Environment.Swagger.ApiName); });
            }
        }

        protected abstract void ConfigureDependencies(IServiceCollection services);
        protected virtual void ConfigureApplicationServices(IServiceCollection services)
        {
        }
        protected abstract void ConfigureEmerald(EmeraldOptions options);
        protected TDbContext CreateDbContext()
        {
            return DbContextFactory.Create<TDbContext>(Configuration.Environment.ApplicationDb.ConnectionString);
        }
    }
}
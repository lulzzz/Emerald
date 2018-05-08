using Emerald.AspNetCore.Common;
using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.Extensions;
using Emerald.AspNetCore.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emerald.AspNetCore
{
    public abstract class StartupBase<TDbContext> where TDbContext : DbContext
    {
        protected StartupBase(IConfiguration configuration)
        {
            Configuration = configuration;
            Environment = new EnvironmentConfigurationSection(configuration);
        }

        protected IConfiguration Configuration { get; }
        protected EnvironmentConfigurationSection Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureDependencies(services);
            services.AddEmerald<TDbContext, ServiceScopeFactory, TransactionScopeFactory<TDbContext>>(Configuration, ConfigureEmerald);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseEmerald();
            env.ApplicationName = Environment.ApplicationName;
            env.EnvironmentName = Environment.Name;
        }

        protected abstract void ConfigureDependencies(IServiceCollection serviceCollection);
        protected abstract void ConfigureEmerald(EmeraldOptions options);
    }
}
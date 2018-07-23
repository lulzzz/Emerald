using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.EntityFrameworkCore;
using Emerald.AspNetCore.Extensions;
using Emerald.AspNetCore.System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IServiceCollection = Microsoft.Extensions.DependencyInjection.IServiceCollection;

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
            services.AddDbContext<TDbContext>(opt => opt.UseSqlServer(Configuration.Environment.ApplicationDb.ConnectionString));

            ConfigureDependencies(services);

            services.AddEmerald<ServiceScopeFactory>(Configuration, opt =>
            {
                opt.SetCommandExecutionStrategy<SqlAzureCommandExecutionStrategy>();
                opt.SetTransactionScopeFactory<TransactionScopeFactory<TDbContext>>();
                ConfigureEmerald(opt);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseEmerald();
            env.ApplicationName = Configuration.Environment.ApplicationName;
            env.EnvironmentName = Configuration.Environment.Name;
        }

        protected abstract void ConfigureDependencies(IServiceCollection serviceCollection);
        protected abstract void ConfigureEmerald(EmeraldOptions options);

        protected TDbContext CreateDbContext()
        {
            return DbContextFactory.Create<TDbContext>(Configuration.Environment.ApplicationDb.ConnectionString);
        }
    }
}
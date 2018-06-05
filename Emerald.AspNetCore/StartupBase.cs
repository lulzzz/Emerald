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
            ApplicationConfiguration = new ApplicationConfiguration(configuration);
        }

        protected IConfiguration Configuration { get; }
        protected IApplicationConfiguration ApplicationConfiguration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TDbContext>(opt => opt.UseSqlServer(ApplicationConfiguration.Environment.ApplicationDb.ConnectionString));
            ConfigureDependencies(services);
            services.AddEmerald<ServiceScopeFactory, TransactionScopeFactory<TDbContext>>(Configuration, ConfigureEmerald);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseEmerald();
            env.ApplicationName = ApplicationConfiguration.Environment.ApplicationName;
            env.EnvironmentName = ApplicationConfiguration.Environment.Name;
        }

        protected abstract void ConfigureDependencies(IServiceCollection serviceCollection);
        protected abstract void ConfigureEmerald(EmeraldOptions options);

        protected TDbContext CreateDbContext()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            dbContextOptionsBuilder.UseSqlServer(ApplicationConfiguration.Environment.ApplicationDb.ConnectionString);
            return (TDbContext)typeof(TDbContext).GetConstructor(new[] { typeof(DbContextOptions) }).Invoke(new object[] { dbContextOptionsBuilder.Options });
        }
    }
}
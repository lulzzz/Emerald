using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.Persistence;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace Emerald.AspNetCore
{
    public abstract class ProgramBase<TStartup, TDbContext, TDbInitializer> where TStartup : StartupBase<TDbContext> where TDbContext : DbContext where TDbInitializer : DbInitializer<TDbContext>, new()
    {
        protected static void Run(string[] args)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false).Build();
            var environment = new EnvironmentConfigurationSection(configuration);
            var builder = WebHost.CreateDefaultBuilder(args).UseStartup<TStartup>();
            ConfiguringLogging(builder, environment);
            ConfiguringUrls(builder, environment);
            var host = builder.Build();
            ConfiguringDatabase(host, environment);
            host.Run();
        }
        private static void ConfiguringLogging(IWebHostBuilder builder, EnvironmentConfigurationSection environment)
        {
            var environmentName = environment.Name;

            Log.Logger =
                string.Equals(environmentName, EnvironmentName.Development, StringComparison.InvariantCultureIgnoreCase) ?
                    new LoggerConfiguration().MinimumLevel.Information().MinimumLevel.Override("Microsoft", LogEventLevel.Information).Enrich.FromLogContext().WriteTo.Console().CreateLogger() :
                    new LoggerConfiguration().MinimumLevel.Information().MinimumLevel.Override("Microsoft", LogEventLevel.Warning).Enrich.FromLogContext().WriteTo.Console().CreateLogger();

            builder.UseSerilog();
        }
        private static void ConfiguringUrls(IWebHostBuilder builder, EnvironmentConfigurationSection environment)
        {
            if (string.Equals(environment.Name, EnvironmentName.Development, StringComparison.InvariantCultureIgnoreCase))
            {
                builder.UseUrls(environment.Host);
            }
        }
        private static void ConfiguringDatabase(IWebHost host, EnvironmentConfigurationSection environment)
        {
            if (environment.ApplicationDb.MigrateDatabaseToLatestVersion)
            {
                using (var scope = host.Services.CreateScope())
                using (var dbContext = scope.ServiceProvider.GetService<TDbContext>())
                {
                    var logger = scope.ServiceProvider.GetService<ILogger<ProgramBase<TStartup, TDbContext, TDbInitializer>>>();

                    try
                    {
                        dbContext.Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Error on migrating database.", ex);
                    }
                }
            }

            if (environment.ApplicationDb.ExecuteSeed)
            {
                using (var scope = host.Services.CreateScope())
                using (var dbContext = scope.ServiceProvider.GetService<TDbContext>())
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    var logger = scope.ServiceProvider.GetService<ILogger<ProgramBase<TStartup, TDbContext, TDbInitializer>>>();
                    var dbInitializer = Activator.CreateInstance<TDbInitializer>();

                    dbInitializer.Initialize();

                    try
                    {
                        dbInitializer.Seed(dbContext);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        logger.LogError("Error on running seed.", ex);
                    }
                }
            }
        }
    }
}
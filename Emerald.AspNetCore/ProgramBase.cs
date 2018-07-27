using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.EntityFrameworkCore;
using Emerald.Utils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.IO;

namespace Emerald.AspNetCore
{
    public abstract class ProgramBase<TStartup, TDbContext, TDbInitializer> where TStartup : StartupBase<TDbContext> where TDbContext : DbContext where TDbInitializer : DbInitializer<TDbContext>, new()
    {
        protected static void Run(string[] args)
        {
            var configurationRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false).Build();
            var configuration = new ApplicationConfiguration(configurationRoot);

            ConfiguringLogging(configuration);
            ConfiguringDatabase(configuration);

            var builder = WebHost.CreateDefaultBuilder(args).UseStartup<TStartup>().UseSerilog();
            ConfiguringDevelopmentHost(builder, configuration);

            var host = builder.Build();
            host.Run();
        }

        private static void ConfiguringLogging(ApplicationConfiguration configuration)
        {
            var environmentName = configuration.Environment.Name;
            var logging = configuration.Environment.Logging;
            var loggerConfiguration = new LoggerConfiguration().MinimumLevel.Is(logging.Level).Enrich.FromLogContext();

            if (logging.Console.Enabled) loggerConfiguration = loggerConfiguration.WriteTo.Console();
            if (logging.ElasticSearch.Enabled) loggerConfiguration = loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(logging.ElasticSearch.NodeUri)) { IndexFormat = logging.ElasticSearch.IndexFormat });
            loggerConfiguration = loggerConfiguration.MinimumLevel.Override("Microsoft", string.Equals(environmentName, EnvironmentName.Development, StringComparison.InvariantCultureIgnoreCase) ? logging.Level : LogEventLevel.Warning);

            Log.Logger = loggerConfiguration.CreateLogger();
        }
        private static void ConfiguringDatabase(ApplicationConfiguration configuration)
        {
            if (configuration.Environment.ApplicationDb.MigrateDatabaseToLatestVersion)
            {
                try
                {
                    using (var dbContext = DbContextFactory.Create<TDbContext>(configuration.Environment.ApplicationDb.ConnectionString))
                    {
                        dbContext.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(LoggerHelper.CreateLog("Error on migrating database."), ex);
                }
            }

            if (configuration.Environment.ApplicationDb.ExecuteSeed)
            {
                try
                {
                    using (var dbContext = DbContextFactory.Create<TDbContext>(configuration.Environment.ApplicationDb.ConnectionString))
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        var dbInitializer = Activator.CreateInstance<TDbInitializer>();

                        dbInitializer.Initialize();

                        try
                        {
                            dbInitializer.Seed(dbContext);
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(LoggerHelper.CreateLog("Error on running db seed."), ex);
                }
            }
        }
        private static void ConfiguringDevelopmentHost(IWebHostBuilder builder, ApplicationConfiguration configuration)
        {
            if (string.Equals(configuration.Environment.Name, EnvironmentName.Development, StringComparison.InvariantCultureIgnoreCase)) builder.UseUrls(configuration.Environment.Development.Host);
        }
    }
}
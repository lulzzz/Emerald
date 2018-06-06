using Emerald.AspNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Emerald.AspNetCore.Persistence
{
    public abstract class DesignTimeDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext> where TDbContext : DbContext
    {
        public TDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false).Build();
            var environmentConfiguration = new EnvironmentConfigurationSection(configuration);
            return DbContextFactory.Create<TDbContext>(environmentConfiguration.ApplicationDb.ConnectionString);
        }
    }
}
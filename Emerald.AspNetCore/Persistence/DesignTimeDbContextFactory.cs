using Emerald.AspNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace Emerald.AspNetCore.Persistence
{
    public abstract class DesignTimeDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext> where TDbContext : DbContext
    {
        public TDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false).Build();
            var environmentConfiguration = new EnvironmentConfigurationSection(configuration);
            var constructor = typeof(TDbContext).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(DbContextOptions) }, null);
            if (constructor == null) throw new ApplicationException($"Cannot find constructor of type {typeof(TDbContext)} with parameter of type {typeof(DbContextOptions)}.");
            var dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.UseSqlServer(environmentConfiguration.ApplicationDb.ConnectionString);
            return (TDbContext)constructor.Invoke(new object[] { dbContextOptionsBuilder.Options });
        }
    }
}
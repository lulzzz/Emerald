using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace Emerald.AspNetCore.EntityFrameworkCore
{
    public static class DbContextFactory
    {
        public static TDbContext Create<TDbContext>(string connectionString) where TDbContext : DbContext
        {
            var constructor = typeof(TDbContext).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(DbContextOptions) }, null);
            if (constructor == null) throw new ApplicationException($"Cannot find constructor of type {typeof(TDbContext)} with parameter of type {typeof(DbContextOptions)}.");
            var dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.UseSqlServer(connectionString);
            return (TDbContext)constructor.Invoke(new object[] { dbContextOptionsBuilder.Options });
        }
    }
}
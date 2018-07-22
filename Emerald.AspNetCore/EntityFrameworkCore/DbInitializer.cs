using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Emerald.AspNetCore.EntityFrameworkCore
{
    public abstract class DbInitializer<TDbContext> where TDbContext : DbContext
    {
        private readonly DbInitializerConfig _config = new DbInitializerConfig();

        protected abstract void Configure(DbInitializerConfig config);

        internal void Initialize()
        {
            Configure(_config);
        }

        internal void Seed(TDbContext dbContext)
        {
            _config.ResourceNameList.ForEach(r => dbContext.Database.ExecuteSqlCommand(GetSql(r)));
        }

        protected virtual string GetSql(string resourceName)
        {
            var assembly = GetType().Assembly;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }

    public sealed class DbInitializerConfig
    {
        internal DbInitializerConfig()
        {
        }

        internal List<string> ResourceNameList { get; } = new List<string>();

        public void AddResourceName(string resourceName)
        {
            ResourceNameList.Add(resourceName);
        }
    }
}
using System.Collections.Generic;

namespace Emerald.AspNetCore.Persistence
{
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
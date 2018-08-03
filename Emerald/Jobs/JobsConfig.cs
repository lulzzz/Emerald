using System;
using System.Collections.Generic;

namespace Emerald.Jobs
{
    public sealed class JobsConfig
    {
        private readonly List<JobInfo> _jobInfoList = new List<JobInfo>();

        internal JobsConfig()
        {
        }

        internal JobInfo[] Jobs => _jobInfoList.ToArray();

        public void AddJob<T>(bool enabled, string expression) where T : IJob
        {
            _jobInfoList.Add(new JobInfo { Type = typeof(T), Enabled = enabled, Expression = expression });
        }
    }

    internal sealed class JobInfo
    {
        public Type Type { get; set; }
        public string Expression { get; set; }
        public bool Enabled { get; set; }
    }
}
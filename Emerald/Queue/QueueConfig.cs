using System;
using System.Collections.Generic;

namespace Emerald.Queue
{
    public sealed class QueueConfig
    {
        internal QueueConfig(string applicationName, string connectionString, long interval, Type eventListenerType)
        {
            ConnectionString = connectionString;
            Interval = interval;
            EventListenerType = eventListenerType;
            QueueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
        }

        internal string ConnectionString { get; }
        internal Type EventListenerType { get; }
        internal List<Type> EventTypeList { get; } = new List<Type>();
        internal long Interval { get; }
        internal QueueDbAccessManager QueueDbAccessManager { get; }
    }
}
using System;
using System.Collections.Generic;

namespace Emerald.Queue
{
    public sealed class QueueConfig
    {
        internal QueueConfig(string applicationName, string connectionString, long interval)
        {
            ConnectionString = connectionString;
            Interval = interval;
            QueueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
        }

        internal string ConnectionString { get; }
        internal Dictionary<Type, Type> EventListenerDictionary { get; } = new Dictionary<Type, Type>();
        internal List<Type> EventListenerTypeList { get; } = new List<Type>();
        internal long Interval { get; }
        internal QueueDbAccessManager QueueDbAccessManager { get; }

        public void AddEventListener<T>() where T : EventListener
        {
            EventListenerTypeList.Add(typeof(T));
        }
    }
}
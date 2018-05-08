using System;
using System.Collections.Generic;

namespace Emerald.Queue
{
    public sealed class QueueConfig
    {
        private readonly List<Type> _eventListenerTypeList = new List<Type>();

        internal QueueConfig(string applicationName, string connectionString, long interval, bool listen)
        {
            ConnectionString = connectionString;
            Interval = interval;
            Listen = listen;
            QueueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
        }

        internal string ConnectionString { get; }
        internal bool Listen { get; }
        internal Type[] EventListenerTypes => _eventListenerTypeList.ToArray();
        internal Dictionary<Type, List<Type>> EventTypes { get; set; }
        internal long Interval { get; }
        internal QueueDbAccessManager QueueDbAccessManager { get; }

        public void AddEventListener<T>() where T : EventListener
        {
            if (!_eventListenerTypeList.Contains(typeof(T))) _eventListenerTypeList.Add(typeof(T));
        }
    }
}
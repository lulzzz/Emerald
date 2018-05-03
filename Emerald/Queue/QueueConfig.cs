using System;
using System.Collections.Generic;

namespace Emerald.Queue
{
    public sealed class QueueConfig
    {
        internal QueueConfig(string applicationName, string connectionString, long interval, Type[] eventListenerTypes, bool listen)
        {
            ConnectionString = connectionString;
            Interval = interval;
            EventListenerTypes = eventListenerTypes;
            Listen = listen;
            QueueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
        }

        internal string ConnectionString { get; }
        internal bool Listen { get; }
        internal Type[] EventListenerTypes { get; }
        internal Dictionary<Type, List<Type>> EventTypes { get; set; }
        internal long Interval { get; }
        internal QueueDbAccessManager QueueDbAccessManager { get; }
    }

    public sealed class QueueListenerConfig
    {
        private readonly List<Type> _eventListenerTypeList = new List<Type>();

        internal QueueListenerConfig()
        {
        }

        internal Type[] EventListenerTypes => _eventListenerTypeList.ToArray();

        public void AddEventListener<T>() where T : EventListener
        {
            if (!_eventListenerTypeList.Contains(typeof(T))) _eventListenerTypeList.Add(typeof(T));
        }
    }
}
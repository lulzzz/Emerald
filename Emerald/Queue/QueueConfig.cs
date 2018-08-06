using System;
using System.Collections.Generic;

namespace Emerald.Queue
{
    public sealed class QueueConfig
    {
        private readonly List<Type> _eventHandlerTypeList = new List<Type>();

        internal QueueConfig(string applicationName, string connectionString, TimeSpan delay, TimeSpan interval, bool listen)
        {
            ConnectionString = connectionString;
            Delay = delay;
            Interval = interval;
            Listen = listen;
            QueueDbAccessManager = new QueueDbAccessManager(applicationName, connectionString);
        }

        internal string ConnectionString { get; }
        internal TimeSpan Delay { get; }
        internal Type[] EventHandlerTypes => _eventHandlerTypeList.ToArray();
        internal TimeSpan Interval { get; }
        internal bool Listen { get; }
        internal QueueDbAccessManager QueueDbAccessManager { get; }

        public QueueConfig AddEventHandler<T>() where T : EventHandler
        {
            _eventHandlerTypeList.Add(typeof(T));
            return this;
        }
    }
}
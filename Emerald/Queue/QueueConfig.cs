using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Emerald.Queue
{
    public sealed class QueueConfig
    {
        private readonly IServiceCollection _serviceCollection;

        internal QueueConfig(string connectionString, IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            ConnectionString = connectionString;
        }

        internal string ConnectionString { get; }
        internal List<Type> EventListenerTypeList { get; } = new List<Type>();

        public void AddEventListener<T>() where T : EventListener
        {
            EventListenerTypeList.Add(typeof(T));
            _serviceCollection.AddScoped<T>();
        }
    }
}
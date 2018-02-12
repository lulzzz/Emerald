using Akka.Actor;
using Emerald.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emerald.Queue
{
    internal sealed class EventListenerActor : ReceiveActor
    {
        private static bool _initialized;

        private readonly QueueDbAccessManager _dbAccessManager;
        private readonly Dictionary<Type, Type> _eventListenerDictionary;
        private readonly long _interval;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public const string ListenCommand = "LISTEN";

        public EventListenerActor(
            QueueDbAccessManager dbAccessManager,
            Dictionary<Type, Type> eventListenerDictionary,
            long interval,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory,
            ITransactionScopeFactory transactionScopeFactory)
        {
            _dbAccessManager = dbAccessManager;
            _eventListenerDictionary = eventListenerDictionary;
            _interval = interval;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;

            Receive<string>(s => s == ListenCommand, s => Listen());
        }

        private void Listen()
        {
            try
            {
                if (!_initialized)
                {
                    _dbAccessManager.CreateQueueDbIfNeeded();
                    _dbAccessManager.RegisterSubscriberIfNeeded();
                    _initialized = true;
                }

                var eventList = _dbAccessManager.GetEvents().ToList();

                foreach (var @event in eventList)
                {
                    if (!_eventListenerDictionary.ContainsKey(@event.GetType())) continue;

                    using (var scope = _serviceScopeFactory.CreateScope())
                    using (var transaction = _transactionScopeFactory.Create(scope))
                    {
                        var eventListener = (EventListener)scope.ServiceProvider.GetService(_eventListenerDictionary[@event.GetType()]);

                        try
                        {
                            eventListener.Handle(@event);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.LogError("Error on handling event.", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on listening events.");
            }

            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(_interval), Self, ListenCommand, Self);
        }
    }
}
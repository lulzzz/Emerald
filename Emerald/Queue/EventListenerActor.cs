using Akka.Actor;
using Emerald.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    internal sealed class EventListenerActor : ReceiveActor
    {
        private static bool _initialized;

        private readonly QueueDbAccessManager _dbAccessManager;
        private readonly Dictionary<Type, Type> _eventListenerDictionary;
        private readonly Dictionary<string, Type> _eventTypeDictionary = new Dictionary<string, Type>();
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

            ReceiveAsync<string>(s => s == ListenCommand, s => Listen());
        }

        private async Task Listen()
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
                    using (var scope = _serviceScopeFactory.CreateScope())
                    using (var transaction = _transactionScopeFactory.Create(scope))
                    {
                        try
                        {
                            var obj = CreateEventObject(@event);
                            if (!_eventListenerDictionary.ContainsKey(obj.GetType())) continue;
                            var eventListener = (EventListener)scope.ServiceProvider.GetService(_eventListenerDictionary[obj.GetType()]);
                            eventListener.Initialize();
                            await eventListener.Handle(obj);
                            transaction.Commit();
                            _logger.LogInformation($"Event handled. Event: {JsonConvert.SerializeObject(@event)}.");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.LogError(ex, $"Error on handling event. Event: {JsonConvert.SerializeObject(@event)}.");
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

        private object CreateEventObject(Event @event)
        {
            if (_eventTypeDictionary.ContainsKey(@event.Type))
            {
                return JsonConvert.DeserializeObject(@event.Body, _eventTypeDictionary[@event.Type]);
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(@event.Type);
                if (type == null) continue;
                _eventTypeDictionary.Add(@event.Type, type);
                return JsonConvert.DeserializeObject(@event.Body, type);
            }

            throw new NotSupportedException($"Cannot find type '{@event.Type}'.");
        }
    }
}
using Akka.Actor;
using Akka.Event;
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
        private readonly Dictionary<string, Type> _eventTypeDictionary;
        private readonly QueueConfig _queueConfig;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;
        public const string ListenCommand = "LISTEN";
        public const string ScheduleNextListenCommand = "SCHEDULENEXTLISTEN";

        public EventListenerActor(QueueConfig queueConfig, IServiceScopeFactory serviceScopeFactory, ITransactionScopeFactory transactionScopeFactory)
        {
            _eventTypeDictionary = queueConfig.EventTypes.ToDictionary(i => i.Key.Name, i => i.Key);
            _queueConfig = queueConfig;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;
            Receive<string>(s => s == ListenCommand, s => Listen().PipeTo(Self));
            Receive<string>(msg => msg == ScheduleNextListenCommand, msg => ScheduleNextListen());
        }

        private async Task<string> Listen()
        {
            var logger = Context.GetLogger();

            try
            {
                if (!_initialized)
                {
                    await _queueConfig.QueueDbAccessManager.CreateQueueDbIfNeeded();
                    await _queueConfig.QueueDbAccessManager.RegisterSubscriberIfNeeded();
                    _initialized = true;
                }

                var eventArray = await _queueConfig.QueueDbAccessManager.GetEvents();
                if (eventArray.Length == 0) return ScheduleNextListenCommand;
                logger.Info($"{eventArray.Length} event(s) received.");

                foreach (var @event in eventArray)
                {
                    if (!_eventTypeDictionary.ContainsKey(@event.Type))
                    {
                        await _queueConfig.QueueDbAccessManager.AddLog(@event.Id, "Success", "Event handler not registered.");
                        continue;
                    }

                    using (var scope = _serviceScopeFactory.CreateScope())
                    using (var transaction = _transactionScopeFactory.Create(scope))
                    {
                        logger.Info($"Starting handle event '{@event.Id}:{@event.Type}'.");

                        try
                        {
                            var eventType = _eventTypeDictionary[@event.Type];
                            var eventObj = JsonConvert.DeserializeObject(@event.Body, eventType);

                            foreach (var eventListenerType in _queueConfig.EventTypes[eventType])
                            {
                                var eventListener = (EventListener)scope.ServiceProvider.GetService(eventListenerType);
                                eventListener.Initialize();
                                await eventListener.Handle(eventObj);
                            }

                            transaction.Commit();
                            logger.Info($"Event '{@event.Id}:{@event.Type}' handled.");
                            await _queueConfig.QueueDbAccessManager.AddLog(@event.Id, "Success", "Event handled successfully.");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            logger.Error(ex, $"Error on handling event '{@event.Id}:{@event.Type}'.");
                            await _queueConfig.QueueDbAccessManager.AddLog(@event.Id, "Error", ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on listening events.");
            }

            return ScheduleNextListenCommand;
        }
        private void ScheduleNextListen()
        {
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(_queueConfig.Interval), Self, ListenCommand, Self);
        }
    }
}
using Akka.Actor;
using Akka.Event;
using Emerald.Abstractions;
using Emerald.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    internal sealed class EventHandlerActor : ReceiveActor
    {
        private readonly Dictionary<string, Type> _eventTypeDictionary;
        private readonly QueueConfig _queueConfig;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public EventHandlerActor(QueueConfig queueConfig, IServiceScopeFactory serviceScopeFactory, ITransactionScopeFactory transactionScopeFactory)
        {
            _eventTypeDictionary = queueConfig.EventTypes.ToDictionary(i => i.Key.Name, i => i.Key);
            _queueConfig = queueConfig;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;
            ReceiveAsync<Event>(Handle);
        }

        private async Task Handle(Event @event)
        {
            var receivedAt = DateTime.UtcNow;
            var logger = Context.GetLogger();

            logger.Info(LoggerHelper.CreateLogContent($"Starting handle event '{@event.Id}:{@event.Type}'."));

            try
            {
                if (!_eventTypeDictionary.ContainsKey(@event.Type))
                {
                    await _queueConfig.QueueDbAccessManager.AddLog(@event.Id, "Missed", "Event handler not registered.", @event.ReadAt, receivedAt, DateTime.UtcNow);
                    return;
                }

                Exception exception = null;

                using (var scope = _serviceScopeFactory.CreateScope())
                using (var transaction = _transactionScopeFactory.Create(scope))
                {
                    try
                    {
                        var eventType = _eventTypeDictionary[@event.Type];
                        var eventObj = JsonHelper.Deserialize(@event.Body, eventType);

                        foreach (var eventListenerType in _queueConfig.EventTypes[eventType])
                        {
                            var eventListener = (EventListener)scope.ServiceProvider.GetService(eventListenerType);
                            eventListener.Initialize();
                            await eventListener.Handle(eventObj);
                        }

                        transaction.Commit();

                        logger.Info(LoggerHelper.CreateLogContent($"Event '{@event.Id}:{@event.Type}' handled."));
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        exception = ex;
                    }
                }

                if (exception == null)
                {
                    await _queueConfig.QueueDbAccessManager.AddLog(@event.Id, "Success", "Event handled successfully.", @event.ReadAt, receivedAt, DateTime.UtcNow);
                }
                else
                {
                    await _queueConfig.QueueDbAccessManager.AddLog(@event.Id, "Error", exception.ToString(), @event.ReadAt, receivedAt, DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, LoggerHelper.CreateLogContent($"Error on handling event '{@event.Id}:{@event.Type}'."));
            }
        }
    }
}
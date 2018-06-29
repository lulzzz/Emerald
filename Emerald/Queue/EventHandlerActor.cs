﻿using Akka.Actor;
using Akka.Event;
using Emerald.Abstractions;
using Emerald.Utils;
using Newtonsoft.Json;
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
            var logger = Context.GetLogger();

            try
            {
                if (!_eventTypeDictionary.ContainsKey(@event.Type))
                {
                    await _queueConfig.QueueDbAccessManager.AddLog(@event.Id, "Success", "Event handler not registered.");
                    return;
                }

                using (var scope = _serviceScopeFactory.CreateScope())
                using (var transaction = _transactionScopeFactory.Create(scope))
                {
                    try
                    {
                        logger.Info(LoggerHelper.CreateLogContent($"Starting handle event '{@event.Id}:{@event.Type}'."));

                        var eventType = _eventTypeDictionary[@event.Type];
                        var eventObj = JsonConvert.DeserializeObject(@event.Body, eventType);

                        foreach (var eventListenerType in _queueConfig.EventTypes[eventType])
                        {
                            var eventListener = (EventListener)scope.ServiceProvider.GetService(eventListenerType);
                            eventListener.Initialize();
                            await eventListener.Handle(eventObj);
                        }

                        transaction.Commit();
                        await _queueConfig.QueueDbAccessManager.AddLog(@event.Id, "Success", "Event handled successfully.");
                        logger.Info(LoggerHelper.CreateLogContent($"Event '{@event.Id}:{@event.Type}' handled."));
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        await _queueConfig.QueueDbAccessManager.AddLog(@event.Id, "Error", ex.ToString());
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, LoggerHelper.CreateLogContent($"Error on handling event '{@event.Id}:{@event.Type}'."));
            }
        }
    }
}
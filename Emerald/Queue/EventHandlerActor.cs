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
            ReceiveAsync<QueueEnvelope>(Handle);
        }

        private async Task Handle(QueueEnvelope envelope)
        {
            var logger = Context.GetLogger();
            envelope.EventProcessingLogBuilder.EventReceived();

            try
            {
                if (!_eventTypeDictionary.ContainsKey(envelope.Event.Type))
                {
                    envelope.EventProcessingLogBuilder.EventHandled();
                    await _queueConfig.QueueDbAccessManager.AddLog(envelope.Event.Id, "Missed", "Event handler not registered.");
                    envelope.EventProcessingLogBuilder.EventLogWrote();
                    envelope.EventProcessingLogBuilder.SetMessage("Event handled successfully.");
                    logger.Info(envelope.EventProcessingLogBuilder.Build());
                }
                else
                {
                    Exception exception = null;

                    try
                    {
                        await RetryHelper.Execute(async () =>
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            using (var transaction = _transactionScopeFactory.Create(scope))
                            {
                                try
                                {
                                    var eventType = _eventTypeDictionary[envelope.Event.Type];
                                    var eventObj = JsonHelper.Deserialize(envelope.Event.Body, eventType);

                                    foreach (var eventListenerType in _queueConfig.EventTypes[eventType])
                                    {
                                        var eventListener = (EventListener)scope.ServiceProvider.GetService(eventListenerType);
                                        eventListener.Initialize();
                                        await eventListener.Handle(eventObj);
                                    }

                                    transaction.Commit();
                                }
                                catch
                                {
                                    transaction.Rollback();
                                    throw;
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    envelope.EventProcessingLogBuilder.EventHandled();

                    if (exception == null)
                    {
                        await _queueConfig.QueueDbAccessManager.AddLog(envelope.Event.Id, "Success", "Event handled successfully.");
                        envelope.EventProcessingLogBuilder.EventLogWrote();
                        envelope.EventProcessingLogBuilder.SetMessage("Event handled successfully.");
                        logger.Info(envelope.EventProcessingLogBuilder.Build());
                    }
                    else
                    {
                        await _queueConfig.QueueDbAccessManager.AddLog(envelope.Event.Id, "Error", exception.ToString());
                        envelope.EventProcessingLogBuilder.EventLogWrote();
                        envelope.EventProcessingLogBuilder.SetMessage("Event handled with error.");
                        logger.Error(exception, envelope.EventProcessingLogBuilder.Build());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, LoggerHelper.CreateLogContent($"Error on handling event '{envelope.Event.Id}:{envelope.Event.Type}'."));
            }
        }
    }
}
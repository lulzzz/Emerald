using Akka.Actor;
using Akka.Event;
using Emerald.Abstractions;
using Emerald.Core;
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
        private readonly Func<CommandExecutor> _commandExecutorFactory;
        private readonly QueueConfig _queueConfig;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public EventHandlerActor(Func<CommandExecutor> commandExecutorFactory, QueueConfig queueConfig, IServiceScopeFactory serviceScopeFactory, ITransactionScopeFactory transactionScopeFactory)
        {
            _eventTypeDictionary = queueConfig.EventTypes.ToDictionary(i => i.Key.Name, i => i.Key);
            _commandExecutorFactory = commandExecutorFactory;
            _queueConfig = queueConfig;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;
            ReceiveAsync<QueueEnvelope>(Handle);
        }

        private async Task Handle(QueueEnvelope envelope)
        {
            var logger = Context.GetLogger();
            var startedAt = DateTime.UtcNow;
            var exceptionList = new List<Exception>();

            try
            {
                if (!_eventTypeDictionary.ContainsKey(envelope.Event.Type))
                {
                    await _queueConfig.QueueDbAccessManager.AddLog(envelope.Event.Id, "Missed", "Event handler not registered.");
                    logger.Info(CreateLogMessage("Event handled successfully.", envelope, startedAt, new EventHandlerInfo[0]));
                    return;
                }

                var eventType = _eventTypeDictionary[envelope.Event.Type];
                var eventObj = JsonHelper.Deserialize(envelope.Event.Body, eventType);
                var eventHandlerList = new List<EventHandlerInfo>();

                foreach (var eventHandlerType in _queueConfig.EventTypes[eventType])
                {
                    var handlerStartedAt = DateTime.UtcNow;
                    var commandExecutor = _commandExecutorFactory.Invoke();
                    var status = "Success";

                    try
                    {
                        var eventHandlerConstructor = eventHandlerType.GetConstructor(Type.EmptyTypes);
                        if (eventHandlerConstructor == null) throw new ApplicationException($"Can not find parameterless constructor in type '{eventHandlerType.FullName}'.");

                        await RetryHelper.Execute(async () =>
                        {
                            var eventHandler = (EventHandler)eventHandlerConstructor.Invoke(new object[0]);
                            eventHandler.Initialize();
                            eventHandler.CommandExecutor = commandExecutor;
                            eventHandler.ServiceScopeFactory = _serviceScopeFactory;
                            eventHandler.TransactionScopeFactory = _transactionScopeFactory;
                            await eventHandler.Handle(eventObj);
                        });
                    }
                    catch (Exception ex)
                    {
                        exceptionList.Add(ex);
                        status = "Failed";
                    }

                    eventHandlerList.Add(new EventHandlerInfo(eventHandlerType.Name, handlerStartedAt, status, commandExecutor.GetInfo()));
                }

                if (exceptionList.Count == 0)
                {
                    await _queueConfig.QueueDbAccessManager.AddLog(envelope.Event.Id, "Success", "Event handled successfully.");
                    logger.Info(CreateLogMessage("Event handled successfully.", envelope, startedAt, eventHandlerList.ToArray()));
                }
                else
                {
                    var exception = new AggregateException(exceptionList);
                    await _queueConfig.QueueDbAccessManager.AddLog(envelope.Event.Id, "Error", exception.ToString());
                    logger.Error(exception, CreateLogMessage("Event handled with errors.", envelope, startedAt, eventHandlerList.ToArray()));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, LoggerHelper.CreateLogContent($"Error on handling event '{envelope.Event.Id}:{envelope.Event.Type}'."));
            }
        }

        private string CreateLogMessage(string message, QueueEnvelope envelope, DateTime startedAt, EventHandlerInfo[] handlers)
        {
            return JsonHelper.Serialize(new
            {
                message,
                eventId = envelope.Event.Id,
                eventType = envelope.Event.Type,
                consistentHashKey = envelope.Event.ConsistentHashKey,
                startedAt,
                time = $"{Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)}ms",
                listener = envelope.Listener,
                handlers
            });
        }
    }
}
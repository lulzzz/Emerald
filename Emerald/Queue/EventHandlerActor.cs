using Akka.Actor;
using Emerald.Core;
using Emerald.Logging;
using Emerald.System;
using Emerald.Utils;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    internal sealed class EventHandlerActor : ReceiveActor
    {
        private readonly Dictionary<string, Tuple<Type, List<Type>>> _eventHandlerDictionary;
        private readonly QueueDbAccessManager _queueDbAccessManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EventHandlerActor(Dictionary<string, Tuple<Type, List<Type>>> eventHandlerDictionary, QueueDbAccessManager queueDbAccessManager, IServiceScopeFactory serviceScopeFactory)
        {
            _eventHandlerDictionary = eventHandlerDictionary;
            _queueDbAccessManager = queueDbAccessManager;
            _serviceScopeFactory = serviceScopeFactory;

            ReceiveAsync<Event>(Handle);
        }

        private async Task Handle(Event @event)
        {
            var startedAt = DateTime.UtcNow;
            var exceptionList = new List<Exception>();
            var eventHandlerInfoList = new List<EventHandlerInfo>();
            var correlationId = @event.Id.ToString();

            try
            {
                if (!_eventHandlerDictionary.ContainsKey(@event.Type))
                {
                    await _queueDbAccessManager.AddLog(@event.Id, "Missed");
                    return;
                }

                var eventType = _eventHandlerDictionary[@event.Type].Item1;
                var eventObj = JsonConvert.DeserializeObject(@event.Body, eventType);

                foreach (var eventHandlerType in _eventHandlerDictionary[@event.Type].Item2)
                {
                    using (var scope = _serviceScopeFactory.Create())
                    {
                        var handlerStartedAt = DateTime.UtcNow;
                        var commandExecutor = (ICommandExecutor)scope.ServiceProvider.GetService(typeof(ICommandExecutor));
                        var result = EventHandlerInfo.SuccessResult;

                        commandExecutor.SetCorrelationId(correlationId);

                        try
                        {
                            var eventHandler = (EventHandler)scope.ServiceProvider.GetService(eventHandlerType);
                            var loggerContext = (LoggerContext)scope.ServiceProvider.GetService(typeof(ILoggerContext));

                            eventHandler.Initialize();
                            loggerContext.SetCorrelationId(correlationId);

                            await eventHandler.Handle(eventObj);
                        }
                        catch (Exception ex)
                        {
                            exceptionList.Add(ex);
                            result = EventHandlerInfo.ErrorResult;
                        }

                        eventHandlerInfoList.Add(new EventHandlerInfo(commandExecutor.GetCommands(), result, handlerStartedAt, eventHandlerType.Name));
                    }
                }

                await _queueDbAccessManager.AddLog(@event.Id, exceptionList.Count == 0 ? "Success" : "Error");
            }
            catch (Exception ex)
            {
                exceptionList.Add(ex);
            }

            var aggregateException = exceptionList.Count == 0 ? null : new AggregateException(exceptionList);
            var level = aggregateException == null ? LogEventLevel.Information : LogEventLevel.Error;
            var logContent = new
            {
                message = aggregateException == null ? "Event handled successfully." : "Event handled with errors",
                correlationId,
                eventId = @event.Id,
                eventType = @event.Type,
                startedAt,
                consistentHashKey = @event.ConsistentHashKey,
                handlingTime = $"{Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)}ms",
                handlers = eventHandlerInfoList.Select(i => new
                {
                    name = i.Type,
                    startedAt = i.StartedAt,
                    result = i.Result,
                    handlingTime = i.HandlingTime,
                    commands = i.Commands.Select(c => new
                    {
                        name = c.GetType().Name,
                        startedAt = c.StartedAt,
                        result = c.Result,
                        consistentHashKey = c.ConsistentHashKey,
                        executionTime = c.ExecutionTime
                    })
                }),
                listener = new
                {
                    cycleId = @event.Listener.CycleId,
                    startedAt = @event.Listener.StartedAt,
                    events = @event.Listener.Events
                }
            }.ToJson(Formatting.Indented);

            Log.Logger.Write(level, aggregateException, "content: {content}, correlationId: {correlationId}", logContent, correlationId);
        }
    }
}
﻿using Akka.Actor;
using Akka.Event;
using Emerald.Utils;
using System;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    internal sealed class EventListenerActor : ReceiveActor
    {
        private static bool _initialized;
        private readonly IActorRef _eventHandlerActor;
        private readonly QueueConfig _queueConfig;
        public const string ListenCommand = "LISTEN";
        public const string ScheduleNextListenCommand = "SCHEDULENEXTLISTEN";

        public EventListenerActor(IActorRef eventHandlerActor, QueueConfig queueConfig)
        {
            _eventHandlerActor = eventHandlerActor;
            _queueConfig = queueConfig;
            ReceiveAsync<string>(s => s == ListenCommand, s => Listen().PipeTo(Self));
            Receive<string>(msg => msg == ScheduleNextListenCommand, msg => ScheduleNextListen());
        }

        private async Task<string> Listen()
        {
            var logger = Context.GetLogger();
            var listenerStartedAt = DateTime.UtcNow;
            DateTime? listenerInitializedDbAt = null;

            try
            {
                if (!_initialized)
                {
                    await _queueConfig.QueueDbAccessManager.CreateQueueDbIfNeeded();
                    await _queueConfig.QueueDbAccessManager.RegisterSubscriberIfNeeded();
                    _initialized = true;
                    listenerInitializedDbAt = DateTime.UtcNow;
                }

                var eventArray = await _queueConfig.QueueDbAccessManager.GetEvents();
                if (eventArray.Length == 0) return ScheduleNextListenCommand;

                foreach (var @event in eventArray)
                {
                    var eventProcessingInfoLogBuilder = new EventProcessingLogBuilder();
                    eventProcessingInfoLogBuilder.Start(listenerStartedAt);
                    eventProcessingInfoLogBuilder.DbInitialized(listenerInitializedDbAt);
                    eventProcessingInfoLogBuilder.EventRead(@event.ReadAt);
                    eventProcessingInfoLogBuilder.SetEventId(@event.Id);
                    eventProcessingInfoLogBuilder.EventSent();
                    _eventHandlerActor.Tell(new QueueEnvelope(@event, eventProcessingInfoLogBuilder));
                }
            }
            catch (Exception ex)
            {
                logger.Error(LoggerHelper.CreateLogContent("Error on listening events.", ex));
            }

            return ScheduleNextListenCommand;
        }
        private void ScheduleNextListen()
        {
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(_queueConfig.Interval), Self, ListenCommand, Self);
        }
    }
}
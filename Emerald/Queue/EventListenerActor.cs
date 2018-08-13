using Akka.Actor;
using Emerald.Utils;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    internal sealed class EventListenerActor : ReceiveActor
    {
        private static bool _initialized;

        private readonly IActorRef _eventHandlerActor;
        private readonly TimeSpan _interval;
        private readonly QueueDbAccessManager _queueDbAccessManager;

        public const string ListenCommand = "LISTEN";
        public const string ScheduleNextListenCommand = "SCHEDULENEXTLISTEN";

        public EventListenerActor(IActorRef eventHandlerActor, TimeSpan interval, QueueDbAccessManager queueDbAccessManager)
        {
            _eventHandlerActor = eventHandlerActor;
            _interval = interval;
            _queueDbAccessManager = queueDbAccessManager;

            ReceiveAsync<string>(s => s == ListenCommand, s => Listen().PipeTo(Self));
            Receive<string>(msg => msg == ScheduleNextListenCommand, msg => ScheduleNextListen());
        }

        private async Task<string> Listen()
        {
            var startedAt = DateTime.UtcNow;
            var cycleId = Guid.NewGuid();
            var exception = default(Exception);
            var eventIdArray = new long[0];

            try
            {
                if (!_initialized)
                {
                    await _queueDbAccessManager.CreateQueueDbIfNeeded();
                    await _queueDbAccessManager.RegisterSubscriberIfNeeded();
                    _initialized = true;
                }

                var eventArray = await _queueDbAccessManager.GetEvents();
                eventIdArray = eventArray.Select(e => e.Id).ToArray();
                var eventListenerInfo = new EventListenerInfo(cycleId, eventIdArray, startedAt);

                foreach (var @event in eventArray)
                {
                    @event.SetListener(eventListenerInfo);
                    _eventHandlerActor.Tell(@event);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var logContent = new
            {
                message = exception == null ? "Listener cycle completed." : "Listener cycle completed with error.",
                cycleId,
                startedAt,
                events = eventIdArray,
                readingTime = $"{Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)}ms"
            };

            Log.Logger.Write(exception == null ? LogEventLevel.Debug : LogEventLevel.Error, exception, "content: {content}", logContent.ToJson(Formatting.Indented));

            return ScheduleNextListenCommand;
        }

        private void ScheduleNextListen()
        {
            Context.System.Scheduler.ScheduleTellOnce(_interval, Self, ListenCommand, Self);
        }
    }
}
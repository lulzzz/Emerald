using Akka.Actor;
using Akka.Event;
using Emerald.Utils;
using Serilog;
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
            var cycleId = Guid.NewGuid();
            var startedAt = DateTime.UtcNow;
            Exception exception = null;
            var received = 0;

            try
            {
                if (!_initialized)
                {
                    await _queueConfig.QueueDbAccessManager.CreateQueueDbIfNeeded();
                    await _queueConfig.QueueDbAccessManager.RegisterSubscriberIfNeeded();
                    _initialized = true;
                }

                var eventArray = await _queueConfig.QueueDbAccessManager.GetEvents();
                received = eventArray.Length;

                foreach (var @event in eventArray)
                {
                    var eventProcessingInfoLogBuilder = new EventListenerInfo(cycleId, startedAt);
                    _eventHandlerActor.Tell(new QueueEnvelope(@event, eventProcessingInfoLogBuilder));
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var logger = Context.GetLogger();

            if (exception == null)
            {
                Log.Logger.Debug(CreateLogMessage("Listener cycle completed successfully.", cycleId, startedAt, received));
            }
            else
            {
                logger.Error(exception, CreateLogMessage("Listener cycle completed with error.", cycleId, startedAt, received));
            }

            return ScheduleNextListenCommand;
        }

        private void ScheduleNextListen()
        {
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(_queueConfig.Interval), Self, ListenCommand, Self);
        }

        private string CreateLogMessage(string message, Guid cycleId, DateTime startedAt, int received)
        {
            return JsonHelper.Serialize(new
            {
                message,
                cycleId,
                startedAt,
                received,
                time = $"{Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)}ms"
            });
        }
    }
}
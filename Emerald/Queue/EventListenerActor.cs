using Akka.Actor;
using Akka.Event;
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
            ReceiveAsync<string>(s => s == ListenCommand, s => Listen());
        }

        private async Task Listen()
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
                if (eventArray.Length != 0)
                {
                    logger.Info($"{eventArray.Length} event(s) received.");
                    _eventHandlerActor.Tell(eventArray);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on listening events.");
            }

            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(_queueConfig.Interval), Self, ListenCommand, Self);
        }
    }
}
using Emerald.Utils;
using System;

namespace Emerald.Queue
{
    internal sealed class EventProcessingLogBuilder
    {
        private DateTime? _listenerInitializedDbAt;
        private double? _dbInitializingTime;
        private DateTime _listenerReadEventAt;
        private double _eventReadingByListenerTime;
        private DateTime _listenerSentEventToHandlerAt;
        private double _eventSendingToHandlerTime;
        private DateTime _handlerReceivedEventAt;
        private double _eventReceivingByHandlerTime;
        private DateTime _handlerHandledEventAt;
        private double _eventHandlingTime;
        private DateTime _handlerWroteLogAt;
        private double _eventLogTime;

        public string Message { get; private set; }
        public long EventId { get; private set; }
        public string EventType { get; private set; }
        public string ConsistentHashKey { get; private set; }
        public DateTime ListenerStartedAt { get; private set; }
        public string DbInitializingTime => _dbInitializingTime == null ? null : $"{_dbInitializingTime}ms";
        public string EventReadingByListenerTime => $"{_eventReadingByListenerTime}ms";
        public string EventSendingToHandlerTime => $"{_eventSendingToHandlerTime}ms";
        public string EventReceivingByHandlerTime => $"{_eventReceivingByHandlerTime}ms";
        public string EventHandlingTime => $"{_eventHandlingTime}ms";
        public string EventLogTime => $"{_eventLogTime}ms";
        public string Total => $"{Math.Round((_handlerWroteLogAt - ListenerStartedAt).TotalMilliseconds)}ms";

        public void Start(DateTime startedAt)
        {
            ListenerStartedAt = startedAt;
        }
        public void DbInitialized(DateTime? dbInitializedAt)
        {
            if (dbInitializedAt == null) return;
            _listenerInitializedDbAt = dbInitializedAt;
            _dbInitializingTime = Math.Round((_listenerInitializedDbAt.Value - ListenerStartedAt).TotalMilliseconds);
        }
        public void EventRead(DateTime readAt)
        {
            _listenerReadEventAt = readAt;
            _eventReadingByListenerTime = Math.Round((_listenerReadEventAt - (_listenerInitializedDbAt ?? ListenerStartedAt)).TotalMilliseconds);
        }
        public void EventSent()
        {
            _listenerSentEventToHandlerAt = DateTime.UtcNow;
            _eventSendingToHandlerTime = Math.Round((_listenerSentEventToHandlerAt - _listenerReadEventAt).TotalMilliseconds);
        }
        public void EventReceived()
        {
            _handlerReceivedEventAt = DateTime.UtcNow;
            _eventReceivingByHandlerTime = Math.Round((_handlerReceivedEventAt - _listenerSentEventToHandlerAt).TotalMilliseconds);
        }
        public void EventHandled()
        {
            _handlerHandledEventAt = DateTime.UtcNow;
            _eventHandlingTime = Math.Round((_handlerHandledEventAt - _handlerReceivedEventAt).TotalMilliseconds);
        }
        public void EventLogWrote()
        {
            _handlerWroteLogAt = DateTime.UtcNow;
            _eventLogTime = Math.Round((_handlerWroteLogAt - _handlerHandledEventAt).TotalMilliseconds);
        }
        public void SetEventInfo(long eventId, string eventType, string consistentHashKey)
        {
            EventId = eventId;
            EventType = eventType;
            ConsistentHashKey = consistentHashKey;
        }
        public void SetMessage(string message)
        {
            Message = message;
        }
        public string Build()
        {
            return JsonHelper.Serialize(this);
        }
    }
}
using Emerald.Utils;
using System;

namespace Emerald.Core
{
    public sealed class CommandProcessingLogBuilder
    {
        private DateTime _executorSentEventToHandlerAt;
        private double _commandSendingToHandlerTime;
        private DateTime _handlerReceivedCommandAt;
        private double _commandReceivingByHandlerTime;
        private DateTime _handlerHandledCommandAt;
        private double _commandHandlingTime;
        private DateTime _executorReceivedResultAt;
        private double _resultReceivingByExecutorTime;

        public string Message { get; private set; }
        public Guid CommandId { get; private set; }
        public string CommandType { get; private set; }
        public string ConsistentHashKey { get; private set; }
        public DateTime ExecutionStartedAt { get; private set; }
        public string CommandSendingToHandlerTime => $"{_commandSendingToHandlerTime}ms";
        public string CommandReceivingByHandlerTime => $"{_commandReceivingByHandlerTime}ms";
        public string CommandHandlingTime => $"{_commandHandlingTime}ms";
        public string ResultReceivingByExecutorTime => $"{_resultReceivingByExecutorTime}ms";
        public string Total => $"{Math.Round((_executorReceivedResultAt - ExecutionStartedAt).TotalMilliseconds)}ms";

        public void Start()
        {
            ExecutionStartedAt = DateTime.UtcNow;
        }
        public void CommandSent()
        {
            _executorSentEventToHandlerAt = DateTime.UtcNow;
            _commandSendingToHandlerTime = Math.Round((_executorSentEventToHandlerAt - ExecutionStartedAt).TotalMilliseconds);
        }
        public void CommandReceived()
        {
            _handlerReceivedCommandAt = DateTime.UtcNow;
            _commandReceivingByHandlerTime = Math.Round((_handlerReceivedCommandAt - _executorSentEventToHandlerAt).TotalMilliseconds);
        }
        public void CommandHandled()
        {
            _handlerHandledCommandAt = DateTime.UtcNow;
            _commandHandlingTime = Math.Round((_handlerHandledCommandAt - _handlerReceivedCommandAt).TotalMilliseconds);
        }
        public void ResultReceived()
        {
            _executorReceivedResultAt = DateTime.UtcNow;
            _resultReceivingByExecutorTime = Math.Round((_executorReceivedResultAt - _handlerHandledCommandAt).TotalMilliseconds);
        }
        public void SetEventInfo(Guid commandId, string commandType, string consistentHashKey)
        {
            CommandId = commandId;
            CommandType = commandType;
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
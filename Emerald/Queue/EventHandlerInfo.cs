using Emerald.Core;
using System;

namespace Emerald.Queue
{
    internal sealed class EventHandlerInfo
    {
        public EventHandlerInfo(string type, DateTime startedAt, string status, CommandInfo[] commands)
        {
            Type = type;
            StartedAt = startedAt;
            Status = status;
            Commands = commands;
            Time = $"{Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)}ms";
        }

        public string Type { get; }
        public DateTime StartedAt { get; }
        public string Status { get; }
        public string Time { get; }
        public CommandInfo[] Commands { get; }
    }
}
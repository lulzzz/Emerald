using System;

namespace Emerald.Core
{
    public sealed class CommandInfo
    {
        public CommandInfo(string type, DateTime startedAt, string status, object consistentHashKey)
        {
            Type = type;
            StartedAt = startedAt;
            Status = status;
            ConsistentHashKey = consistentHashKey;
            Time = $"{Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)}ms";
        }

        public string Type { get; }
        public DateTime StartedAt { get; }
        public string Status { get; }
        public object ConsistentHashKey { get; }
        public string Time { get; }
    }
}
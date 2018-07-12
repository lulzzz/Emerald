using Akka.Routing;
using System;

namespace Emerald.Queue
{
    internal sealed class Event : IConsistentHashable
    {
        public Event(long id, string type, string body, string source, DateTime publishedAt, string consistentHashKey, DateTime readAt)
        {
            Id = id;
            Type = type;
            Body = body;
            Source = source;
            PublishedAt = publishedAt;
            ConsistentHashKey = consistentHashKey;
            ReadAt = readAt;
        }

        public long Id { get; }
        public string Type { get; }
        public string Body { get; }
        public string Source { get; }
        public DateTime PublishedAt { get; }
        public string ConsistentHashKey { get; }
        public DateTime ReadAt { get; }

        object IConsistentHashable.ConsistentHashKey => ConsistentHashKey;
    }
}
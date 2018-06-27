using Akka.Routing;
using System;

namespace Emerald.Queue
{
    public sealed class Event : IConsistentHashable
    {
        public Event(long id, string type, string body, string source, DateTime publishedAt, string consistentHashKey)
        {
            Id = id;
            Type = type;
            Body = body;
            Source = source;
            PublishedAt = publishedAt;
            ConsistentHashKey = consistentHashKey;
        }

        public long Id { get; }
        public string Type { get; }
        public string Body { get; }
        public string Source { get; }
        public DateTime PublishedAt { get; }
        public string ConsistentHashKey { get; }

        object IConsistentHashable.ConsistentHashKey => ConsistentHashKey;
    }
}
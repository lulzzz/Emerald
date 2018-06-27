using System;

namespace Emerald.Queue
{
    public sealed class Event
    {
        public Event(long id, string type, string body, string source, DateTime publishedAt)
        {
            Id = id;
            Type = type;
            Body = body;
            Source = source;
            PublishedAt = publishedAt;
        }

        public long Id { get; }
        public string Type { get; }
        public string Body { get; }
        public string Source { get; }
        public DateTime PublishedAt { get; }
    }
}
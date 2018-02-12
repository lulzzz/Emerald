using System;

namespace Emerald.Queue
{
    public sealed class Event
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Body { get; set; }
        public string Source { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
using Emerald.Queue;

namespace Emerald.AspNetCore.Common
{
    public sealed class QueueOptions
    {
        private readonly QueueConfig _config;

        internal QueueOptions(QueueConfig config)
        {
            _config = config;
        }

        public void AddEventListener<T>() where T : EventListener
        {
            _config.AddEventListener<T>();
        }
    }
}
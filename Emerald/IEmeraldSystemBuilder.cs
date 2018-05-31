using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;

namespace Emerald
{
    public interface IEmeraldSystemBuilder
    {
        void AddCommandHandler<T>() where T : CommandHandler;
        void AddJob<T>(string cronTab) where T : class, IJob;
        QueueConfig UseQueue(string connectionString, long interval, bool listen);
        EmeraldSystem Build();
    }
}
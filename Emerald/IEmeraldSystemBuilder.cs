using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;

namespace Emerald
{
    public interface IEmeraldSystemBuilder
    {
        IEmeraldSystemBuilder AddCommandHandler<T>() where T : CommandHandler;
        IEmeraldSystemBuilder AddJob<T>(string cron) where T : class, IJob;
        IEmeraldSystemBuilder UseQueue<T>(string connectionString, long interval) where T : EventListener;
        EmeraldSystem Build();
    }
}
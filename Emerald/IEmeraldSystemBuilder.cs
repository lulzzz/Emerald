using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;
using System;

namespace Emerald
{
    public interface IEmeraldSystemBuilder
    {
        IEmeraldSystemBuilder AddCommandHandler<T>() where T : CommandHandler;
        IEmeraldSystemBuilder AddJob<T>(string cron) where T : class, IJob;
        IEmeraldSystemBuilder UseQueue(string connectionString, long interval, Action<QueueConfig> configure);
        EmeraldSystem Build();
    }
}
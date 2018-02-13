using Akka.Actor;
using Emerald.Abstractions;
using System;
using System.Threading.Tasks;

namespace Emerald.Jobs
{
    internal sealed class JobActor : ReceiveActor
    {
        private readonly string _cron;
        private readonly Type _jobType;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public const string ExecuteJobCommand = "EXECUTE";

        public JobActor(
            string cron,
            Type jobType,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory,
            ITransactionScopeFactory transactionScopeFactory)
        {
            _cron = cron;
            _jobType = jobType;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;

            ReceiveAsync<string>(msg => msg == ExecuteJobCommand, msg => ExecuteJob());
        }

        private async Task ExecuteJob()
        {
            _logger.LogInformation($"Job '{_jobType.Name}' started.");

            using (var scope = _serviceScopeFactory.CreateScope())
            using (var transaction = _transactionScopeFactory.Create(scope))
            {
                var job = (IJob)scope.ServiceProvider.GetService(_jobType);

                try
                {
                    await job.Execute();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Error on running job.");
                }
            }

            Context.System.Scheduler.ScheduleTellOnce(GetDelay(_cron), Self, ExecuteJobCommand, Self);

            _logger.LogInformation($"Job '{_jobType.Name}' finished.");
        }

        public static TimeSpan GetDelay(string cron)
        {
            var now = DateTime.UtcNow;
            var next = NCrontab.CrontabSchedule.Parse(cron).GetNextOccurrence(now);
            var duration = (next - now).Duration();
            return duration;
        }
    }
}
using Akka.Actor;
using Akka.Event;
using Emerald.Abstractions;
using System;
using System.Threading.Tasks;

namespace Emerald.Jobs
{
    internal sealed class JobActor : ReceiveActor
    {
        private readonly TimeSpan _delay;
        private readonly Type _jobType;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public const string ExecuteJobCommand = "EXECUTEJOB";
        public const string ScheduleJobCommand = "SCHEDULEJOB";

        public JobActor(string crontab, Type jobType, IServiceScopeFactory serviceScopeFactory, ITransactionScopeFactory transactionScopeFactory)
        {
            _delay = GetDelay(crontab);
            _jobType = jobType;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;
            Receive<string>(msg => msg == ExecuteJobCommand, msg => ExecuteJob().PipeTo(Self));
            Receive<string>(msg => msg == ScheduleJobCommand, msg => ScheduleJob());
        }

        private async Task<string> ExecuteJob()
        {
            var logger = Context.GetLogger();
            logger.Info($"Job '{_jobType.Name}' started.");

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
                    logger.Error(ex, "Error on job execution.");
                }
            }

            logger.Info($"Job '{_jobType.Name}' finished.");

            return ScheduleJobCommand;
        }
        private void ScheduleJob()
        {
            Context.System.Scheduler.ScheduleTellOnce(_delay, Self, ExecuteJobCommand, Self);
        }
        private TimeSpan GetDelay(string crontab)
        {
            var now = DateTime.UtcNow;
            var next = NCrontab.CrontabSchedule.Parse(crontab).GetNextOccurrence(now);
            var duration = (next - now).Duration();
            return duration;
        }
    }
}
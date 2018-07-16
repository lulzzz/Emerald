using Akka.Actor;
using Akka.Event;
using Emerald.Abstractions;
using Emerald.Core;
using Emerald.Utils;
using System;
using System.Threading.Tasks;

namespace Emerald.Jobs
{
    internal sealed class JobActor : ReceiveActor
    {
        private readonly TimeSpan _delay;
        private readonly Type _jobType;
        private readonly CommandExecutor _commandExecutor;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public const string ExecuteJobCommand = "EXECUTEJOB";
        public const string ScheduleJobCommand = "SCHEDULEJOB";

        public JobActor(string crontab, Type jobType, CommandExecutor commandExecutor, IServiceScopeFactory serviceScopeFactory, ITransactionScopeFactory transactionScopeFactory)
        {
            _delay = GetDelay(crontab);
            _jobType = jobType;
            _commandExecutor = commandExecutor;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;
            Receive<string>(msg => msg == ExecuteJobCommand, msg => ExecuteJob().PipeTo(Self));
            Receive<string>(msg => msg == ScheduleJobCommand, msg => ScheduleJob());
        }

        private async Task<string> ExecuteJob()
        {
            var logger = Context.GetLogger();
            logger.Info(LoggerHelper.CreateLogContent($"Job '{_jobType.Name}' started."));

            try
            {
                var jobConstructor = _jobType.GetConstructor(Type.EmptyTypes);
                if (jobConstructor == null) throw new ApplicationException($"Can not find parameterless constructor in type '{_jobType.FullName}'.");
                var job = (Job)jobConstructor.Invoke(new object[0]);
                job.CommandExecutor = _commandExecutor;
                job.ServiceScopeFactory = _serviceScopeFactory;
                job.TransactionScopeFactory = _transactionScopeFactory;
                await job.Execute();
            }
            catch (Exception ex)
            {
                logger.Error(ex, LoggerHelper.CreateLogContent("Error on job execution."));
            }

            logger.Info(LoggerHelper.CreateLogContent($"Job '{_jobType.Name}' finished."));

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
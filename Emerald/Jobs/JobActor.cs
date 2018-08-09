using Akka.Actor;
using Emerald.Core;
using Emerald.Logging;
using Emerald.System;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Jobs
{
    internal sealed class JobActor : ReceiveActor
    {
        private readonly string _expression;
        private readonly Type _jobType;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public const string ExecuteJobCommand = "EXECUTEJOB";
        public const string ScheduleJobCommand = "SCHEDULEJOB";

        public JobActor(string expression, Type jobType, IServiceScopeFactory serviceScopeFactory)
        {
            _expression = expression;
            _jobType = jobType;
            _serviceScopeFactory = serviceScopeFactory;

            Receive<string>(msg => msg == ExecuteJobCommand, msg => ExecuteJob().PipeTo(Self));
            Receive<string>(msg => msg == ScheduleJobCommand, msg => ScheduleJob());
        }

        private async Task<string> ExecuteJob()
        {
            var correlationId = Guid.NewGuid().ToString();
            var startedAt = DateTime.UtcNow;
            var exception = default(Exception);
            ICommandInfo[] commandInfoArray;

            using (var scope = _serviceScopeFactory.Create())
            {
                var commandExecutor = (ICommandExecutor)scope.ServiceProvider.GetService(typeof(ICommandExecutor));
                var loggerContext = (LoggerContext)scope.ServiceProvider.GetService(typeof(ILoggerContext));

                commandExecutor.SetCorrelationId(correlationId);
                loggerContext.SetCorrelationId(correlationId);

                try
                {
                    var job = (IJob)scope.ServiceProvider.GetService(_jobType);
                    await job.Execute();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                commandInfoArray = commandExecutor.GetCommands();
            }

            var logContent = new
            {
                message = exception == null ? "Job executed successfully." : "Job executed with errors.",
                correlationId,
                jobType = _jobType.Name,
                startedAt,
                executionTime = $"{Math.Round((DateTime.UtcNow - startedAt).TotalMilliseconds)}ms",
                commands = commandInfoArray.Select(c => new
                {
                    name = c.GetType().Name,
                    startedAt = c.StartedAt,
                    result = c.Result,
                    consistentHashKey = c.ConsistentHashKey,
                    executionTime = c.ExecutionTime
                })
            };

            Log.Logger.Write(exception == null ? LogEventLevel.Information : LogEventLevel.Error, exception, "{@content}", new object[] { logContent });

            return ScheduleJobCommand;
        }
        private void ScheduleJob()
        {
            var now = DateTime.UtcNow;
            var next = NCrontab.CrontabSchedule.Parse(_expression).GetNextOccurrence(now);
            var duration = (next - now).Duration();
            Context.System.Scheduler.ScheduleTellOnce(duration, Self, ExecuteJobCommand, Self);
        }
    }
}
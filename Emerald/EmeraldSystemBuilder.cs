using Akka.Actor;
using Akka.Routing;
using Emerald.Abstractions;
using Emerald.Common;
using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;
using System;
using System.Collections.Generic;

namespace Emerald
{
    public sealed class EmeraldSystemBuilder
    {
        private readonly string _applicationName;
        private readonly IServiceCollection _serviceCollection;
        private readonly List<Type> _commandHandlerTypeList = new List<Type>();
        private readonly List<Tuple<Type, string>> _jobTypeList = new List<Tuple<Type, string>>();
        private QueueConfig _queueConfig;

        public EmeraldSystemBuilder(string applicationName, IServiceCollection serviceCollection)
        {
            _applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _serviceCollection.AddSingleton(new CommandExecutor());
            _serviceCollection.AddSingleton(new EventPublisher());
        }

        public EmeraldSystemBuilder AddCommandHandler<T>() where T : CommandHandler
        {
            _commandHandlerTypeList.Add(typeof(T));
            _serviceCollection.AddScoped<T>();
            return this;
        }
        public EmeraldSystemBuilder AddJob<T>(string cron) where T : class, IJob
        {
            _jobTypeList.Add(new Tuple<Type, string>(typeof(T), cron));
            _serviceCollection.AddScoped<T>();
            return this;
        }
        public EmeraldSystemBuilder UseQueue(string connectionString, long interval, Action<QueueConfig> configure)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            _queueConfig = new QueueConfig(connectionString, interval, _serviceCollection);
            configure(_queueConfig);
            return this;
        }

        public EmeraldSystem Build(ILogger logger, IServiceScopeFactory serviceScopeFactory, ITransactionScopeFactory transactionScopeFactory)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (serviceScopeFactory == null) throw new ArgumentNullException(nameof(serviceScopeFactory));
            if (transactionScopeFactory == null) throw new ArgumentNullException(nameof(transactionScopeFactory));

            const string akkaConfig =
                @"akka {
                    loglevel=INFO,
                    loggers=[""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
                }";

            var actorSystem = ActorSystem.Create(_applicationName, akkaConfig);

            Registry.ActorSystem = actorSystem;

            var commandHandlerActorDictionary = new Dictionary<Type, IActorRef>();

            foreach (var commandHandlerType in _commandHandlerTypeList)
            {
                var commandHandlerActorProps = Props.Create(() => new CommandHandlerActor(commandHandlerType, serviceScopeFactory, transactionScopeFactory));
                var commandHandlerActor = actorSystem.ActorOf(commandHandlerActorProps.WithRouter(new ConsistentHashingPool(1000)));
                var commandTypes = GetCommandTypes(commandHandlerType, serviceScopeFactory);
                commandTypes.ForEach(t => commandHandlerActorDictionary.Add(t, commandHandlerActor));
            }

            Registry.CommandHandlerActorDictionary = commandHandlerActorDictionary;

            foreach (var jobType in _jobTypeList)
            {
                var jobActorProps = Props.Create(() => new JobActor(jobType.Item2, jobType.Item1, logger, serviceScopeFactory, transactionScopeFactory));
                var jobActor = actorSystem.ActorOf(jobActorProps);
                actorSystem.Scheduler.ScheduleTellOnce(JobActor.GetDelay(jobType.Item2), jobActor, JobActor.ExecuteJobCommand, ActorRefs.NoSender);
            }

            if (_queueConfig != null && _queueConfig.EventListenerTypeList.Count > 0)
            {
                var eventListenerDictionary = new Dictionary<Type, Type>();

                foreach (var eventListenerType in _queueConfig.EventListenerTypeList)
                {
                    var eventTypes = GetEventTypes(eventListenerType, serviceScopeFactory);
                    eventTypes.ForEach(t => eventListenerDictionary.Add(t, eventListenerType));
                }

                var queueDbAccessManager = new QueueDbAccessManager(_applicationName, _queueConfig.ConnectionString);
                var eventListenerActorProps = Props.Create(() => new EventListenerActor(queueDbAccessManager, eventListenerDictionary, _queueConfig.Interval, logger, serviceScopeFactory, transactionScopeFactory));
                var eventListenerActor = actorSystem.ActorOf(eventListenerActorProps);

                Registry.QueueDbAccessManager = queueDbAccessManager;

                actorSystem.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(5), eventListenerActor, EventListenerActor.ListenCommand, ActorRefs.NoSender);
            }

            return new EmeraldSystem(actorSystem);
        }

        private List<Type> GetCommandTypes(Type commandHandlerType, IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var commandHandler = (CommandHandler)scope.ServiceProvider.GetService(commandHandlerType);
                commandHandler.Initialize();
                return commandHandler.GetCommandTypes();
            }
        }
        private List<Type> GetEventTypes(Type eventListenerType, IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var eventListener = (EventListener)scope.ServiceProvider.GetService(eventListenerType);
                eventListener.Initialize();
                return eventListener.GetEventTypes();
            }
        }
    }
}
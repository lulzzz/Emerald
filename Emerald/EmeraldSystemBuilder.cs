﻿using Akka.Actor;
using Akka.Routing;
using Emerald.Abstractions;
using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;
using System;
using System.Collections.Generic;

namespace Emerald
{
    public sealed class EmeraldSystemBuilder<TServiceScopeFactory, TTransactionScopeFactory> : IEmeraldSystemBuilder where TServiceScopeFactory : class, IServiceScopeFactory where TTransactionScopeFactory : class, ITransactionScopeFactory
    {
        private readonly string _applicationName;
        private readonly List<Type> _commandHandlerTypeList = new List<Type>();
        private readonly List<Tuple<Type, string>> _jobTypeList = new List<Tuple<Type, string>>();
        private QueueConfig _queueConfig;
        private readonly IServiceCollection _serviceCollection;

        public EmeraldSystemBuilder(string applicationName, IServiceCollection serviceCollection)
        {
            _applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        public IEmeraldSystemBuilder AddCommandHandler<T>() where T : CommandHandler
        {
            _commandHandlerTypeList.Add(typeof(T));
            return this;
        }
        public IEmeraldSystemBuilder AddJob<T>(string crontab) where T : class, IJob
        {
            _jobTypeList.Add(new Tuple<Type, string>(typeof(T), crontab));
            return this;
        }
        public IEmeraldSystemBuilder UseQueue(string connectionString, long interval, Action<QueueConfig> configure)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval), interval, "Interval must be greater than 0.");
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            _queueConfig = new QueueConfig(_applicationName, connectionString, interval);
            configure(_queueConfig);
            return this;
        }

        public EmeraldSystem Build()
        {
            const string akkaConfig =
                "akka { " +
                    "stdout-loglevel = INFO, " +
                    "log-config-on-start = on, " +
                    "loglevel=INFO, " +
                    "loggers=[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"] " +
                "}";

            var actorSystem = ActorSystem.Create(_applicationName, akkaConfig);
            var commandHandlerActorDictionary = new Dictionary<Type, IActorRef>();

            _serviceCollection.AddScoped<IServiceScopeFactory, TServiceScopeFactory>();
            _serviceCollection.AddScoped<ITransactionScopeFactory, TTransactionScopeFactory>();
            _commandHandlerTypeList.ForEach(_serviceCollection.AddScoped);
            _jobTypeList.ForEach(j => _serviceCollection.AddScoped(j.Item1));
            _queueConfig?.EventListenerTypeList.ForEach(_serviceCollection.AddScoped);
            _serviceCollection.AddSingleton(new CommandExecutor(actorSystem, commandHandlerActorDictionary));
            _serviceCollection.AddSingleton(new EventPublisher(_queueConfig?.QueueDbAccessManager));

            var serviceProvider = _serviceCollection.BuildServiceProvider();
            var serviceScopeFactory = (IServiceScopeFactory)serviceProvider.GetService(typeof(IServiceScopeFactory));
            var transactionScopeFactory = (ITransactionScopeFactory)serviceProvider.GetService(typeof(ITransactionScopeFactory));

            foreach (var commandHandlerType in _commandHandlerTypeList)
            {
                var commandHandlerActorProps = Props.Create(() => new CommandHandlerActor(commandHandlerType, serviceScopeFactory, transactionScopeFactory));
                var commandHandlerActor = actorSystem.ActorOf(commandHandlerActorProps.WithRouter(new ConsistentHashingPool(1000)));
                var commandTypes = GetCommandTypes(commandHandlerType, serviceScopeFactory);
                commandTypes.ForEach(t => commandHandlerActorDictionary.Add(t, commandHandlerActor));
            }

            foreach (var jobType in _jobTypeList)
            {
                var jobActorProps = Props.Create(() => new JobActor(jobType.Item2, jobType.Item1, serviceScopeFactory, transactionScopeFactory));
                var jobActor = actorSystem.ActorOf(jobActorProps);
                jobActor.Tell(JobActor.ScheduleJobCommand, ActorRefs.NoSender);
            }

            if (_queueConfig != null && _queueConfig.EventListenerTypeList.Count > 0)
            {
                foreach (var eventListenerType in _queueConfig.EventListenerTypeList)
                {
                    var eventTypes = GetEventTypes(eventListenerType, serviceScopeFactory);
                    eventTypes.ForEach(t => _queueConfig.EventListenerDictionary.Add(t, eventListenerType));
                }

                var eventListenerActorProps = Props.Create(() => new EventListenerActor(_queueConfig, serviceScopeFactory, transactionScopeFactory));
                var eventListenerActor = actorSystem.ActorOf(eventListenerActorProps);
                eventListenerActor.Tell(EventListenerActor.ScheduleNextListenCommand);
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
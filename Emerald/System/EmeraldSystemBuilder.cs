﻿using Akka.Actor;
using Akka.Routing;
using Akka.Util.Internal;
using Emerald.Core;
using Emerald.Jobs;
using Emerald.Logging;
using Emerald.Queue;
using System;
using System.Collections.Generic;

namespace Emerald.System
{
    public sealed class EmeraldSystemBuilder
    {
        private const string AkkaConfig =
            "akka { " +
                "stdout-loglevel = INFO, " +
                "log-config-on-start = on, " +
                "loglevel=INFO, " +
                "loggers=[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"] " +
            "}";

        private readonly ActorSystem _actorSystem;
        private readonly string _applicationName;
        private readonly IServiceCollection _serviceCollection;

        private readonly Dictionary<Type, Tuple<Type, IActorRef>> _commandHandlerActorDictionary = new Dictionary<Type, Tuple<Type, IActorRef>>();
        private readonly List<Type> _commandHandlerTypeList = new List<Type>();
        private QueueConfig _queueConfig;
        private JobsConfig _jobsConfig;

        private Type _commandExecutionStrategyType = typeof(DefaultCommandExecutionStrategy);
        private readonly Type _serviceScopeFactoryType;
        private Type _transactionScopeFactoryType = typeof(DefaultTransactionScopeFactory);

        private EmeraldSystemBuilder(string applicationName, IServiceCollection serviceCollection, Type serviceScopeFactoryType)
        {
            _actorSystem = ActorSystem.Create(applicationName, AkkaConfig);
            _applicationName = applicationName;
            _serviceCollection = serviceCollection;
            _serviceScopeFactoryType = serviceScopeFactoryType;
        }

        internal void AddCommandHandler<T>() where T : CommandHandler
        {
            _commandHandlerTypeList.Add(typeof(T));
        }
        internal JobsConfig UseJobs()
        {
            _jobsConfig = new JobsConfig();
            return _jobsConfig;
        }
        internal QueueConfig UseQueue(string connectionString, TimeSpan delay, TimeSpan interval, bool listen)
        {
            _queueConfig = new QueueConfig(_applicationName, connectionString, delay, interval, listen);
            return _queueConfig;
        }

        internal void SetCommandExecutionStrategy(Type type)
        {
            _commandExecutionStrategyType = type;
        }
        internal void SetTransactionScopeFactory(Type type)
        {
            _transactionScopeFactoryType = type;
        }

        internal void RegisterDependencies()
        {
            _serviceCollection.AddScoped(typeof(CommandExecutionStrategy), _commandExecutionStrategyType);
            _serviceCollection.AddScoped(typeof(IServiceScopeFactory), _serviceScopeFactoryType);
            _serviceCollection.AddScoped(typeof(ITransactionScopeFactory), _transactionScopeFactoryType);
            _serviceCollection.AddScoped(typeof(ILoggerContext), typeof(LoggerContext));

            _commandHandlerTypeList.ForEach(_serviceCollection.AddScoped);

            _jobsConfig?.Jobs.ForEach(j => _serviceCollection.AddScoped(j.Type));

            if (_queueConfig != null)
            {
                _queueConfig.EventHandlerTypes.ForEach(_serviceCollection.AddScoped);
                _serviceCollection.AddSingleton<IEventPublisher>(new EventPublisher(_queueConfig.QueueDbAccessManager));
            }

            _serviceCollection.AddScoped<ICommandExecutor>(() => new CommandExecutor(_commandHandlerActorDictionary));
        }

        internal EmeraldSystem Build(IServiceProvider serviceProvider)
        {
            var commandExecutionStategy = (CommandExecutionStrategy)serviceProvider.GetService(typeof(CommandExecutionStrategy));
            var serviceScopeFactory = (IServiceScopeFactory)serviceProvider.GetService(typeof(IServiceScopeFactory));
            var transactionScopeFactory = (ITransactionScopeFactory)serviceProvider.GetService(typeof(ITransactionScopeFactory));

            foreach (var commandHandlerType in _commandHandlerTypeList)
            {
                var commandHandlerActorProps = Props.Create(() => new CommandHandlerActor(commandExecutionStategy, commandHandlerType, serviceScopeFactory, transactionScopeFactory));
                var commandHandlerActor = _actorSystem.ActorOf(commandHandlerActorProps.WithRouter(new ConsistentHashingPool(100)));
                var commandTypes = GetCommandTypes(commandHandlerType, serviceScopeFactory);
                commandTypes.ForEach(t => _commandHandlerActorDictionary.Add(t, new Tuple<Type, IActorRef>(commandHandlerType, commandHandlerActor)));
            }

            if (_jobsConfig != null)
            {
                foreach (var job in _jobsConfig.Jobs)
                {
                    if (!job.Enabled) continue;
                    var jobActorProps = Props.Create(() => new JobActor(job.Expression, job.Type, serviceScopeFactory));
                    var jobActor = _actorSystem.ActorOf(jobActorProps);
                    jobActor.Tell(JobActor.ScheduleJobCommand, ActorRefs.NoSender);
                }
            }

            if (_queueConfig != null && _queueConfig.Listen && _queueConfig.EventHandlerTypes.Length > 0)
            {
                var eventHandlerDictionary = GetEventTypes(_queueConfig.EventHandlerTypes, serviceScopeFactory);

                var eventHandlerActorProps = Props.Create(() => new EventHandlerActor(eventHandlerDictionary, _queueConfig.QueueDbAccessManager, serviceScopeFactory));
                var eventHandlerActor = _actorSystem.ActorOf(eventHandlerActorProps.WithRouter(new ConsistentHashingPool(100)));

                var eventListenerActorProps = Props.Create(() => new EventListenerActor(eventHandlerActor, _queueConfig.Interval, _queueConfig.QueueDbAccessManager));
                var eventListenerActor = _actorSystem.ActorOf(eventListenerActorProps);

                _actorSystem.Scheduler.ScheduleTellOnce(_queueConfig.Delay, eventListenerActor, EventListenerActor.ScheduleNextListenCommand, ActorRefs.NoSender);
            }

            return new EmeraldSystem(_actorSystem);
        }

        private List<Type> GetCommandTypes(Type commandHandlerType, IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.Create())
            {
                var commandHandler = (CommandHandler)scope.ServiceProvider.GetService(commandHandlerType);
                commandHandler.Initialize();
                return commandHandler.GetCommandTypes();
            }
        }
        private Dictionary<string, Tuple<Type, List<Type>>> GetEventTypes(Type[] eventHandlerTypes, IServiceScopeFactory serviceScopeFactory)
        {
            var dictionary = new Dictionary<string, Tuple<Type, List<Type>>>();

            using (var scope = serviceScopeFactory.Create())
            {
                foreach (var eventHandlerType in eventHandlerTypes)
                {
                    var eventHandler = (Queue.EventHandler)scope.ServiceProvider.GetService(eventHandlerType);

                    eventHandler.Initialize();

                    foreach (var eventType in eventHandler.GetEventTypes())
                    {
                        if (dictionary.ContainsKey(eventType.Name))
                        {
                            dictionary[eventType.Name].Item2.Add(eventHandlerType);
                        }
                        else
                        {
                            dictionary.Add(eventType.Name, new Tuple<Type, List<Type>>(eventType, new List<Type> { eventHandlerType }));
                        }
                    }
                }
            }

            return dictionary;
        }

        public static EmeraldSystemBuilderFirstStepConfig Create<TServiceScopeFactory>(string applicationName, IServiceCollection serviceCollection) where TServiceScopeFactory : class, IServiceScopeFactory
        {
            if (applicationName == null) throw new ArgumentNullException(nameof(applicationName));
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            return new EmeraldSystemBuilderFirstStepConfig(new EmeraldSystemBuilder(applicationName, serviceCollection, typeof(TServiceScopeFactory)));
        }
    }

    public class EmeraldSystemBuilderFirstStepConfig
    {
        private readonly EmeraldSystemBuilder _emeraldSystemBuilder;

        internal EmeraldSystemBuilderFirstStepConfig(EmeraldSystemBuilder emeraldSystemBuilder)
        {
            _emeraldSystemBuilder = emeraldSystemBuilder;
        }

        public EmeraldSystemBuilderFirstStepConfig AddCommandHandler<T>() where T : CommandHandler
        {
            _emeraldSystemBuilder.AddCommandHandler<T>();
            return this;
        }
        public EmeraldSystemBuilderFirstStepConfig UseJobs(Action<JobsConfig> configure)
        {
            var jobsConfig = _emeraldSystemBuilder.UseJobs();
            configure(jobsConfig);
            return this;
        }
        public EmeraldSystemBuilderFirstStepConfig UseQueue(string connectionString, TimeSpan delay, TimeSpan interval, bool listen, Action<QueueConfig> configure)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            var queueConfig = _emeraldSystemBuilder.UseQueue(connectionString, delay, interval, listen);
            configure(queueConfig);
            return this;
        }
        public EmeraldSystemBuilderFirstStepConfig SetTransactionScopeFactory<TTransactionScopeFactory>() where TTransactionScopeFactory : class, ITransactionScopeFactory
        {
            _emeraldSystemBuilder.SetTransactionScopeFactory(typeof(TTransactionScopeFactory));
            return this;
        }
        public EmeraldSystemBuilderFirstStepConfig SetCommandExecutionStrategy<TCommandExecutionStrategy>() where TCommandExecutionStrategy : CommandExecutionStrategy
        {
            _emeraldSystemBuilder.SetCommandExecutionStrategy(typeof(TCommandExecutionStrategy));
            return this;
        }
        public EmeraldSystemBuilderSecondStepConfig RegisterDependencies()
        {
            _emeraldSystemBuilder.RegisterDependencies();
            return new EmeraldSystemBuilderSecondStepConfig(_emeraldSystemBuilder);
        }
    }
    public class EmeraldSystemBuilderSecondStepConfig
    {
        private readonly EmeraldSystemBuilder _emeraldSystemBuilder;

        internal EmeraldSystemBuilderSecondStepConfig(EmeraldSystemBuilder emeraldSystemBuilder)
        {
            _emeraldSystemBuilder = emeraldSystemBuilder;
        }

        public EmeraldSystem Build(IServiceProvider serviceProvider)
        {
            return _emeraldSystemBuilder.Build(serviceProvider);
        }
    }
}
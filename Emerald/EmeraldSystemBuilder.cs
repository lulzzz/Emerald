using Akka.Actor;
using Akka.Routing;
using Akka.Util.Internal;
using Emerald.Abstractions;
using Emerald.Application;
using Emerald.Core;
using Emerald.Jobs;
using Emerald.Queue;
using System;
using System.Collections.Generic;

namespace Emerald
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
        private readonly Dictionary<Type, IActorRef> _commandHandlerActorDictionary = new Dictionary<Type, IActorRef>();
        private readonly List<Type> _commandHandlerTypeList = new List<Type>();
        private readonly List<Tuple<Type, string>> _jobTypeList = new List<Tuple<Type, string>>();
        private QueueConfig _queueConfig;
        private readonly IServiceCollection _serviceCollection;
        private readonly Type _serviceScopeFactoryType;
        private readonly Type _transactionScopeFactoryType;

        private EmeraldSystemBuilder(string applicationName, IServiceCollection serviceCollection, Type serviceScopeFactoryType, Type transactionScopeFactoryType)
        {
            _applicationName = applicationName;
            _serviceCollection = serviceCollection;
            _serviceScopeFactoryType = serviceScopeFactoryType;
            _transactionScopeFactoryType = transactionScopeFactoryType;
            _actorSystem = ActorSystem.Create(_applicationName, AkkaConfig);
        }

        internal void AddCommandHandler<T>() where T : CommandHandler
        {
            _commandHandlerTypeList.Add(typeof(T));
        }
        internal void AddJob<T>(string cronTab) where T : class, IJob
        {
            _jobTypeList.Add(new Tuple<Type, string>(typeof(T), cronTab));
        }
        internal QueueConfig UseQueue(string connectionString, long interval, bool listen)
        {
            _queueConfig = new QueueConfig(_applicationName, connectionString, interval, listen);
            return _queueConfig;
        }
        internal void RegisterDependencies()
        {
            _serviceCollection.AddScoped(typeof(IServiceScopeFactory), _serviceScopeFactoryType);
            _serviceCollection.AddScoped(typeof(ITransactionScopeFactory), _transactionScopeFactoryType);
            _commandHandlerTypeList.ForEach(_serviceCollection.AddScoped);
            _jobTypeList.ForEach(j => _serviceCollection.AddScoped(j.Item1));
            _queueConfig?.EventListenerTypes.ForEach(_serviceCollection.AddScoped);
            _serviceCollection.AddSingleton(new CommandExecutor(_actorSystem, _commandHandlerActorDictionary));
            _serviceCollection.AddSingleton(new EventPublisher(_queueConfig?.QueueDbAccessManager));
        }
        internal EmeraldSystem Build()
        {
            var serviceProvider = _serviceCollection.BuildServiceProvider();
            var serviceScopeFactory = (IServiceScopeFactory)serviceProvider.GetService(typeof(IServiceScopeFactory));
            var transactionScopeFactory = (ITransactionScopeFactory)serviceProvider.GetService(typeof(ITransactionScopeFactory));

            foreach (var commandHandlerType in _commandHandlerTypeList)
            {
                var commandHandlerActorProps = Props.Create(() => new CommandHandlerActor(commandHandlerType, serviceScopeFactory, transactionScopeFactory));
                var commandHandlerActor = _actorSystem.ActorOf(commandHandlerActorProps.WithRouter(new ConsistentHashingPool(1000)));
                var commandTypes = GetCommandTypes(commandHandlerType, serviceScopeFactory);
                commandTypes.ForEach(t => _commandHandlerActorDictionary.Add(t, commandHandlerActor));
            }

            foreach (var jobType in _jobTypeList)
            {
                var jobActorProps = Props.Create(() => new JobActor(jobType.Item2, jobType.Item1, serviceScopeFactory, transactionScopeFactory));
                var jobActor = _actorSystem.ActorOf(jobActorProps);
                jobActor.Tell(JobActor.ScheduleJobCommand, ActorRefs.NoSender);
            }

            if (_queueConfig != null && _queueConfig.Listen && _queueConfig.EventListenerTypes.Length > 0)
            {
                _queueConfig.EventTypes = GetEventTypes(_queueConfig.EventListenerTypes, serviceScopeFactory);
                var eventHandlerActorProps = Props.Create(() => new EventHandlerActor(_queueConfig, serviceScopeFactory, transactionScopeFactory)).WithRouter(new RoundRobinPool(1000));
                var eventHandlerActor = _actorSystem.ActorOf(eventHandlerActorProps);
                var eventListenerActorProps = Props.Create(() => new EventListenerActor(eventHandlerActor, _queueConfig));
                var eventListenerActor = _actorSystem.ActorOf(eventListenerActorProps);
                eventListenerActor.Tell(EventListenerActor.ScheduleNextListenCommand);
            }

            return new EmeraldSystem(_actorSystem);
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
        private Dictionary<Type, List<Type>> GetEventTypes(Type[] eventListenerTypes, IServiceScopeFactory serviceScopeFactory)
        {
            var dictionary = new Dictionary<Type, List<Type>>();

            using (var scope = serviceScopeFactory.CreateScope())
            {
                foreach (var eventListenerType in eventListenerTypes)
                {
                    var eventListener = (EventListener)scope.ServiceProvider.GetService(eventListenerType);
                    eventListener.Initialize();

                    foreach (var eventType in eventListener.GetEventTypes())
                    {
                        if (dictionary.ContainsKey(eventType))
                        {
                            dictionary[eventType].Add(eventListenerType);
                        }
                        else
                        {
                            dictionary.Add(eventType, new List<Type> { eventListenerType });
                        }
                    }
                }
            }

            return dictionary;
        }

        public static EmeraldSystemBuilderFirstStepConfig Create<TServiceScopeFactory, TTransactionScopeFactory>(string applicationName, IServiceCollection serviceCollection) where TServiceScopeFactory : class, IServiceScopeFactory where TTransactionScopeFactory : class, ITransactionScopeFactory
        {
            if (ValidationHelper.IsNullOrEmptyOrWhiteSpace(applicationName)) throw new ArgumentException("'applicationName' is required.", nameof(applicationName));
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            return new EmeraldSystemBuilderFirstStepConfig(new EmeraldSystemBuilder(applicationName, serviceCollection, typeof(TServiceScopeFactory), typeof(TTransactionScopeFactory)));
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
        public EmeraldSystemBuilderFirstStepConfig AddJob<T>(string cronTab) where T : class, IJob
        {
            if (cronTab == null) throw new ArgumentNullException(nameof(cronTab));
            _emeraldSystemBuilder.AddJob<T>(cronTab);
            return this;
        }
        public EmeraldSystemBuilderFirstStepConfig UseQueue(string connectionString, long interval, bool listen, Action<QueueConfig> configure)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval), interval, "Interval must be greater than 0.");
            var queueConfig = _emeraldSystemBuilder.UseQueue(connectionString, interval, listen);
            configure(queueConfig);
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

        public EmeraldSystem Build()
        {
            return _emeraldSystemBuilder.Build();
        }
    }
}
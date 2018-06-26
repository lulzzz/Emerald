using Akka.Actor;
using Akka.Routing;
using Akka.Util.Internal;
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

        public EmeraldSystemBuilder(string applicationName)
        {
            _applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            _actorSystem = ActorSystem.Create(_applicationName, AkkaConfig);
        }

        public void AddCommandHandler<T>() where T : CommandHandler
        {
            _commandHandlerTypeList.Add(typeof(T));
        }
        public void AddJob<T>(string cronTab) where T : class, IJob
        {
            if (cronTab == null) throw new ArgumentNullException(nameof(cronTab));
            _jobTypeList.Add(new Tuple<Type, string>(typeof(T), cronTab));
        }
        public QueueConfig UseQueue(string connectionString, long interval, bool listen)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval), interval, "Interval must be greater than 0.");
            _queueConfig = new QueueConfig(_applicationName, connectionString, interval, listen);
            return _queueConfig;
        }
        public void RegisterDependencies(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IServiceScopeFactory, TServiceScopeFactory>();
            serviceCollection.AddScoped<ITransactionScopeFactory, TTransactionScopeFactory>();
            _commandHandlerTypeList.ForEach(serviceCollection.AddScoped);
            _jobTypeList.ForEach(j => serviceCollection.AddScoped(j.Item1));
            _queueConfig?.EventListenerTypes.ForEach(serviceCollection.AddScoped);
            serviceCollection.AddSingleton(new CommandExecutor(_actorSystem, _commandHandlerActorDictionary));
            serviceCollection.AddSingleton(new EventPublisher(_queueConfig?.QueueDbAccessManager));
        }
        public EmeraldSystem Build(IServiceProvider serviceProvider)
        {
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
                var eventListenerActorProps = Props.Create(() => new EventListenerActor(_queueConfig, serviceScopeFactory, transactionScopeFactory));
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
    }
}
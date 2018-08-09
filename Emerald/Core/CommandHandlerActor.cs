using Akka.Actor;
using Emerald.Common;
using Emerald.Logging;
using Emerald.System;
using System;
using System.Threading.Tasks;

namespace Emerald.Core
{
    internal sealed class CommandHandlerActor : ReceiveActor
    {
        private readonly CommandExecutionStrategy _commandExecutionStrategy;
        private readonly Type _commandHandlerType;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public CommandHandlerActor(CommandExecutionStrategy commandExecutionStrategy, Type commandHandlerType, IServiceScopeFactory serviceScopeFactory, ITransactionScopeFactory transactionScopeFactory)
        {
            _commandExecutionStrategy = commandExecutionStrategy;
            _commandHandlerType = commandHandlerType;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;

            ReceiveAsync<Command>(Handle);
        }

        public async Task Handle(Command command)
        {
            command.Started();

            var exception = default(Exception);
            var isError = false;
            var output = default(object);

            try
            {
                await _commandExecutionStrategy.Execute(async () =>
                {
                    using (var scope = _serviceScopeFactory.Create())
                    using (var transaction = _transactionScopeFactory.Create(scope))
                    {
                        var commandHandler = (CommandHandler)scope.ServiceProvider.GetService(_commandHandlerType);
                        var loggerContext = (LoggerContext)scope.ServiceProvider.GetService(typeof(ILoggerContext));

                        commandHandler.Initialize();
                        loggerContext.SetCorrelationId(command.AsCommandInfo().CorrelationId);

                        try
                        {
                            output = await commandHandler.Handle(command);

                            if (output is IOperationResult operationResult && operationResult.IsError)
                            {
                                isError = true;
                                transaction.Rollback();
                            }
                            else
                            {
                                transaction.Commit();
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                isError = true;
                exception = ex;
            }

            command.Completed(exception, output, isError ? Command.ErrorResult : Command.SuccessResult);

            Context.Sender.Tell(command);
        }
    }
}
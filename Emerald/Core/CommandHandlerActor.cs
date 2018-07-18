using Akka.Actor;
using Emerald.Abstractions;
using Emerald.Application;
using System;
using System.Threading.Tasks;

namespace Emerald.Core
{
    internal sealed class CommandHandlerActor : ReceiveActor
    {
        private readonly Type _commandHandlerType;
        private readonly ICommandExecutionStrategyFactory _commandExecutionStrategyFactory;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public CommandHandlerActor(
            Type commandHandlerType,
            ICommandExecutionStrategyFactory commandExecutionStrategyFactory,
            IServiceScopeFactory serviceScopeFactory,
            ITransactionScopeFactory transactionScopeFactory)
        {
            _commandHandlerType = commandHandlerType;
            _commandExecutionStrategyFactory = commandExecutionStrategyFactory;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;
            ReceiveAsync<Command>(Handle);
        }

        public async Task Handle(Command command)
        {
            var startedAt = DateTime.UtcNow;
            var status = "Success";
            Exception exception = null;
            object output = null;

            try
            {
                await _commandExecutionStrategyFactory.Create().Execute(async () =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    using (var transaction = _transactionScopeFactory.Create(scope))
                    {
                        var commandHandler = (CommandHandler)scope.ServiceProvider.GetService(_commandHandlerType);
                        commandHandler.Initialize();

                        try
                        {
                            output = await commandHandler.Handle(command);

                            if (output is IOperationResult operationResult && operationResult.IsError)
                            {
                                status = "Failed";
                                transaction.Rollback();
                            }
                            else
                            {
                                transaction.Commit();
                            }
                        }
                        catch
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (InvalidOperationException)
                            {
                            }

                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                status = "Failed";
                exception = ex;
            }

            var commandInfo = new CommandInfo(command.GetType().Name, startedAt, status, command.GetConsistentHashKey());
            var commandExecutionResult = new CommandExecutionResult(output, exception, commandInfo);
            Context.Sender.Tell(commandExecutionResult);
        }
    }
}
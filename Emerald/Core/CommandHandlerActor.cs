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
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public CommandHandlerActor(Type commandHandlerType, IServiceScopeFactory serviceScopeFactory, ITransactionScopeFactory transactionScopeFactory)
        {
            _commandHandlerType = commandHandlerType;
            _serviceScopeFactory = serviceScopeFactory;
            _transactionScopeFactory = transactionScopeFactory;

            ReceiveAsync<Command>(Handle);
        }

        public async Task Handle(Command command)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            using (var transaction = _transactionScopeFactory.Create(scope))
            {
                var commandHandler = (CommandHandler)scope.ServiceProvider.GetService(_commandHandlerType);
                var commandExecutionResult = new CommandExecutionResult { CommandId = command.Id };

                commandHandler.Initialize();

                try
                {
                    commandExecutionResult.Output = await commandHandler.Handle(command);

                    if (commandExecutionResult.Output is IOperationResult operationResult && operationResult.IsError)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    commandExecutionResult.Exception = ex;
                    transaction.Rollback();
                }

                Context.System.EventStream.Publish(commandExecutionResult);
            }
        }
    }
}
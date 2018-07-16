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
            ReceiveAsync<CommandEnvelope>(Handle);
        }

        public async Task Handle(CommandEnvelope envelope)
        {
            envelope.CommandProcessingLogBuilder.CommandReceived();
            var commandExecutionResult = new CommandExecutionResult { CommandId = envelope.Command.Id };

            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                using (var transaction = _transactionScopeFactory.Create(scope))
                {
                    var commandHandler = (CommandHandler)scope.ServiceProvider.GetService(_commandHandlerType);
                    commandHandler.Initialize();

                    try
                    {
                        commandExecutionResult.Output = await commandHandler.Handle(envelope.Command);

                        if (commandExecutionResult.Output is IOperationResult operationResult && operationResult.IsError)
                        {
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
            }
            catch (Exception ex)
            {
                commandExecutionResult.Exception = ex;
            }

            envelope.CommandProcessingLogBuilder.CommandHandled();

            Context.Sender.Tell(commandExecutionResult);
        }
    }
}
namespace Emerald.Abstractions
{
    public interface ICommandExecutionStrategyFactory
    {
        ICommandExecutionStrategy Create();
    }
}
using System;
using System.Threading.Tasks;

namespace Emerald.Abstractions
{
    public interface ICommandExecutionStrategy
    {
        Task Execute(Func<Task> handler);
    }
}
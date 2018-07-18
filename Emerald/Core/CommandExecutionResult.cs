using System;

namespace Emerald.Core
{
    internal sealed class CommandExecutionResult
    {
        public CommandExecutionResult(object output, Exception exception, CommandInfo info)
        {
            Output = output;
            Exception = exception;
            Info = info;
        }

        public object Output { get; }
        public Exception Exception { get; }
        public CommandInfo Info { get; }
    }
}
using System;

namespace Emerald.Core
{
    internal sealed class CommandExecutionResult
    {
        public Guid CommandId { get; set; }
        public object Output { get; set; }
        public Exception Exception { get; set; }
    }
}
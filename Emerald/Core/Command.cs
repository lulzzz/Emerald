using Akka.Routing;
using System;

namespace Emerald.Core
{
    public abstract class Command : IConsistentHashable, ICommandInfo
    {
        private Exception _exception;
        private string _executionTime;
        private object _output;
        private DateTime _startedAt;
        private string _result;

        internal const string ErrorResult = "Error";
        internal const string SuccessResult = "Success";

        public virtual object ConsistentHashKey { get; } = null;

        Exception ICommandInfo.Exception => _exception;
        string ICommandInfo.ExecutionTime => _executionTime;
        object ICommandInfo.Output => _output;
        DateTime ICommandInfo.StartedAt => _startedAt;
        string ICommandInfo.Result => _result;

        internal void Started()
        {
            _startedAt = DateTime.UtcNow;
        }
        internal void Completed(Exception exception, object output, string result)
        {
            _exception = exception;
            _executionTime = $"{Math.Round((DateTime.UtcNow - _startedAt).TotalMilliseconds)}ms";
            _output = output;
            _result = result;
        }

        public ICommandInfo AsCommandInfo() => this;
    }

    public interface ICommandInfo
    {
        object ConsistentHashKey { get; }
        Exception Exception { get; }
        string ExecutionTime { get; }
        object Output { get; }
        DateTime StartedAt { get; }
        string Result { get; }
    }
}
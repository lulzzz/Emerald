using Serilog;

namespace Emerald.Logging
{
    internal sealed class LoggerContext : ILoggerContext
    {
        public string CorrelationId { get; private set; }
        public ILogger Logger => Log.Logger;

        public void SetCorrelationId(string correlationId)
        {
            CorrelationId = correlationId;
        }
    }

    public interface ILoggerContext
    {
        string CorrelationId { get; }
        ILogger Logger { get; }
    }
}
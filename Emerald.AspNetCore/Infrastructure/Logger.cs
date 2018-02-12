using Microsoft.Extensions.Logging;
using System;

namespace Emerald.AspNetCore.Infrastructure
{
    internal sealed class Logger : Abstractions.ILogger
    {
        private readonly ILogger _logger;

        public Logger(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public void LogError(Exception ex, string message)
        {
            _logger.LogError(ex, message);
        }
    }
}
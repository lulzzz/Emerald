using System;

namespace Emerald.Abstractions
{
    public interface ILogger
    {
        void LogError(Exception ex, string message);
    }
}
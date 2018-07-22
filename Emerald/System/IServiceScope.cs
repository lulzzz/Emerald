using System;

namespace Emerald.System
{
    public interface IServiceScope : IDisposable
    {
        IServiceProvider ServiceProvider { get; }
    }
}
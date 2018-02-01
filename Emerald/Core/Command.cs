using Akka.Routing;
using System;

namespace Emerald.Core
{
    public abstract class Command : IConsistentHashable
    {
        internal Guid Id { get; } = Guid.NewGuid();
        protected virtual object ConsistentHashKey { get; } = null;
        object IConsistentHashable.ConsistentHashKey => ConsistentHashKey;
    }
}
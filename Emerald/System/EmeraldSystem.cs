using Akka.Actor;
using System;
using System.Threading.Tasks;

namespace Emerald.System
{
    public sealed class EmeraldSystem : IDisposable
    {
        private readonly ActorSystem _actorSystem;

        internal EmeraldSystem(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public Task Terminate()
        {
            return _actorSystem.Terminate();
        }

        public void Dispose()
        {
            _actorSystem.Dispose();
        }
    }
}
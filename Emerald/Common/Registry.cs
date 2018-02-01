using Akka.Actor;
using Emerald.Queue;
using System;
using System.Collections.Generic;

namespace Emerald.Common
{
    internal static class Registry
    {
        public static ActorSystem ActorSystem { get; set; }
        public static Dictionary<Type, IActorRef> CommandHandlerActorDictionary { get; set; }
        public static QueueDbAccessManager QueueDbAccessManager { get; set; }
    }
}
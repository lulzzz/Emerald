using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Core
{
    public abstract class CommandHandler
    {
        private readonly CommandHandlerConfig _config = new CommandHandlerConfig();

        protected abstract void Configure(CommandHandlerConfig config);

        internal void Initialize()
        {
            Configure(_config);
        }

        internal Task<object> Handle(object command)
        {
            return _config.CommandHandlerDictionary[command.GetType()](command);
        }

        internal List<Type> GetCommandTypes()
        {
            return _config.CommandHandlerDictionary.Keys.ToList();
        }
    }
}
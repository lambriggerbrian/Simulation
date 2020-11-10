using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Simulation.CommandSystem
{
    public class CommandHandler
    {
        private readonly ICommandDefinitions _definitions;
        private readonly ICommandTranslator _translator;
        private readonly World _world;

        public CommandHandler(ICommandDefinitions definitions, ICommandTranslator translator, World world)
        {
            _definitions = definitions;
            _translator = translator;
            _world = world;
        }

        public void HandleCommand(object input)
        {
            var args = _translator.Translate(input);
            if (args != null)
            {
                _world.SendCommand(_definitions.CreateCommand(args));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.CommandSystem
{
    public interface ICommandDefinitions
    {
        public Command CreateCommand(List<string> args);

        public bool HasDefinition(String commandType);
    }
}

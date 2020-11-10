using System;
using System.Collections.Generic;

namespace Simulation.CommandSystem
{
    public class Command
    {
        public static Command None = new Command();



        public readonly int CommandType = 0;
        public readonly Action CommandAction;
        private readonly List<string> args;

        public Command()
        {
        }

        public Command(List<string> args)
        {
            if (args == null) throw new ArgumentNullException("Command args must be non-null");
            if (args.Count < 1) throw new ArgumentOutOfRangeException("Command must have >= 1 arg");
            this.args = args;
            CommandType = Convert.ToInt32(this.args[0]);
        }

        public Command(int commandType, Action commandAction, List<string> args)
        {
            CommandType = commandType;
            CommandAction = commandAction;
            this.args = args;
        }

        public List<string> Args => args.GetRange(1, args.Count - 1);

        public override String ToString() => String.Join(" ", args);
    }
}

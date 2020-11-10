using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Simulation.CommandSystem
{
    public class BasicCommandDefinitions : ICommandDefinitions
    {
        public readonly Dictionary<string, CommandType> StringToCommandType = new Dictionary<string, CommandType>
        {
            {"create", CommandType.Create },
            {"register", CommandType.Register },
            {"get", CommandType.Get },
            {"move", CommandType.Move },
        };

        private readonly Dictionary<CommandType, ActionCreator> creators;

        private readonly ILogger<BasicCommandDefinitions> _logger;

        private readonly IEntityManager _manager;

        private readonly ComponentManagerCollection _collection;

        public BasicCommandDefinitions(ILoggerFactory logger, IEntityManager manager, ComponentManagerCollection collection)
        {
            _logger = logger.CreateLogger<BasicCommandDefinitions>();
            _manager = manager;
            _collection = collection;

            creators = new Dictionary<CommandType, ActionCreator>
            {
                { CommandType.Create, CreateCommandAction },
                { CommandType.Register, RegisterCommandAction },
                { CommandType.Get, GetCommandAction },
                { CommandType.Move, MoveCommandAction }
            };
        }

        private delegate Action ActionCreator(List<string> args);

        public enum CommandType
        {
            None,
            Error,
            Create,
            Register,
            Get,
            Move
        }

        public Command CreateCommand(List<string> args)
        {
            CommandType commandType;
            if (!StringToCommandType.TryGetValue(args[0], out commandType))
            {
                _logger.LogError($"Undefined command received: {args[0]}");
                return Command.None;
            }
            List<string> argsOnly = args.GetRange(1, args.Count - 1);
            Action commandAction = creators[commandType](argsOnly);
            return new Command((int)commandType, commandAction, args);
        }

        public Boolean HasDefinition(String commandType) => throw new NotImplementedException();

        private Action MoveCommandAction(List<string> args)
        {
            UInt32 id;
            Vector3 vector;
            if (args.Count < 4)
            {
                _logger.LogError($"Move command requires >= 4 args: EntityId XDelta YDelta ZDelta");
                return null;
            }
            if (!ValidateId(args[0], out id)) return null;
            if (!ParseVector(args.GetRange(1, args.Count - 1), out vector)) return null;
            TransformComponentManager manager = _collection.GetManagerByName("transform") as TransformComponentManager;
            if (manager != null) return () => manager.Translate(Convert.ToInt32(args[0]), vector);
            else return null;
        }

        private Action GetCommandAction(List<string> args)
        {
            UInt32 id;
            if (args.Count < 1)
            {
                _logger.LogError($"Get command requires >= 1 args: either an Entity id or a manager name and Entity id;");
            }
            else if (args.Count == 1)
            {
                if (!ValidateId(args[0], out id)) return null;
                var living = _manager.Alive(id) ? "alive" : "not alive";
                _logger.LogInformation($"Entity {args[0]} is {living}");
            }
            else if (args.Count > 1)
            {
                IComponentManager manager = _collection.GetManagerByName(args[0]);
                if (manager == null) return null;
                if (!ValidateId(args[1], out id)) return null;
                _logger.LogInformation($"{manager.GetInstance(id)}");
            }
            return null;
        }

        private Action CreateCommandAction(List<string> args) => () => _manager.Create();

        private Action RegisterCommandAction(List<string> args)
        {
            UInt32 id;
            if (!ValidateId(args[1], out id)) return null;
            if (args.Count >= 2) return () => _collection.GetManagerByName(args[0].ToString())?.RegisterComponent(Convert.ToUInt32(args[1]));
            else _logger.LogError($"Register command requires >= 2 args: the manager type and entity id.");
            return null;
        }

        private bool ValidateId(string input, out UInt32 id)
        {
            var success = UInt32.TryParse(input, out id);
            if (!success)
            {
                _logger.LogError($"Invalid Entity id: {input}");
                return false;
            }
            return true;
        }

        private bool ParseVector(List<string> input, out Vector3 vector)
        {
            Single[] values = new Single[3];
            bool success = true;
            for (var i = 0; i < 3; i++)
            {
                bool parseSuccess = Single.TryParse(input[i], out values[i]);
                if (!parseSuccess)
                {
                    _logger.LogError($"Invalid int value: {input[i]}");
                    success = false;
                }
            }
            vector = new Vector3(values[0], values[1], values[2]);
            return success;
        }
    }
}

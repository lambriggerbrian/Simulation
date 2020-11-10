using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Simulation.CommandSystem;

namespace Simulation
{
    public class World
    {
        private readonly Queue<Command> commandQueue = new Queue<Command>();
        private readonly ILogger<World> _logger;
        private readonly IEntityManager _entityManager;
        private readonly ComponentManagerCollection _componentManagers;

        public World(ILogger<World> logger, IEntityManager manager, ComponentManagerCollection componentManagers)
        {
            _logger = logger;
            _entityManager = manager;
            _componentManagers = componentManagers;
        }

        public void SendCommand(Command command)
        {
            if (command == Command.None || command.CommandAction == null) return;
            commandQueue.Enqueue(command);
            _logger.LogInformation($"Command '{command}' received.");
        }

        public void Simulate(float timestep)
        {
            if (commandQueue.Count > 0)
            {
                commandQueue.Dequeue().CommandAction.Invoke();
            }
            // TODO: RUN SYSTEMS HERE
        }
    }
}

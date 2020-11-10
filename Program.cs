using System;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Simulation
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var factory = createLoggerFactory();
            EntityManager entityManager = new EntityManager(factory);
            TransformComponentManager transformManager = new TransformComponentManager(factory, entityManager);
            Entity entity0 = entityManager.Create();
            Entity entity1 = entityManager.Create();
            transformManager.RegisterComponent(entity0.Id);
            transformManager.Translate(0, new Vector3(1, 1, 1));
            transformManager.RegisterComponent(entity1.Id, transformManager.EntityToInstanceIndex(entity0.Id));
            transformManager.Translate(0, new Vector3(1, 1, 1));
            Console.ReadKey();
        }

        private static ILoggerFactory createLoggerFactory()
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Default", LogLevel.Error)
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddConsole();
            });
            return factory;
        }
    }
}

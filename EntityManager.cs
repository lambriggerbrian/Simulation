using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Simulation
{
    public class EntityManager : IEntityManager
    {
        #region STATIC_VALUES

        /// <summary>
        /// Number of bits used for the index (Low-order).
        /// </summary>
        public static readonly Int32 ENTITY_INDEX_BITS = 24;

        /// <summary>
        /// Bitmask for the index of the id.
        /// </summary>
        public static readonly UInt32 ENTITY_INDEX_MASK = (UInt32)((1 << ENTITY_INDEX_BITS) - 1);

        /// <summary>
        /// Number of bits used for the generation (high-order).
        /// </summary>
        public static readonly Int32 ENTITY_GENERATION_BITS = 8;

        /// <summary>
        /// Bitmask for the generation of the id.
        /// </summary>
        public static readonly UInt32 ENTITY_GENERATION_MASK = (UInt32)((1 << ENTITY_GENERATION_BITS) - 1);

        /// <summary>
        /// Number of released indices before reuse of any index.
        /// </summary>
        public static readonly UInt32 MINIMUM_FREE_INDICES = 1024;

        /// <summary>
        /// A list of bytes representing the number of times or
        /// "generations" this index has been used.
        /// </summary>
        private readonly List<Byte> indices = new List<Byte>();

        /// <summary>
        /// A queue of indices that have been released from destroyed entities.
        /// </summary>
        private readonly Queue<Int32> freeIndices = new Queue<Int32>();

        /// <summary>
        /// Helper function that returns the index portion of an id.
        /// </summary>
        /// <param name="id">A 32-bit unsigned id.</param>
        /// <returns>A 32-bit signed index.</returns>
        public static Int32 IdToIndex(UInt32 id) => (Int32)(id & ENTITY_INDEX_MASK);

        /// <summary>
        /// Helper function that returns the generation portion of an id.
        /// </summary>
        /// <param name="id">A 32-bit unsigned id.</param>
        /// <returns>The generation as a byte.</returns>
        public static Byte IdToGeneration(UInt32 id) => (Byte)((id >> ENTITY_INDEX_BITS) & ENTITY_GENERATION_MASK);

        #endregion STATIC_VALUES

        private readonly ILogger<EntityManager> _logger;
        private int entityCount = 0;

        public EntityManager()
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Default", LogLevel.Error)
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddConsole();
            });
            _logger = factory.CreateLogger<EntityManager>();
        }

        public EntityManager(ILoggerFactory factory) => _logger = factory.CreateLogger<EntityManager>();

        public Boolean Alive(UInt32 id)
        {
            Int32 index = IdToIndex(id);
            if (index >= indices.Count)
            {
                _logger.LogDebug($"Invalid id");
                return false;
            }
            Byte generation = IdToGeneration(id);
            return generation == this.indices[index];
        }

        public Entity Create()
        {
            Int32 index;
            if (this.freeIndices.Count > MINIMUM_FREE_INDICES)
            {
                index = this.freeIndices.Dequeue();
            }
            else
            {
                index = this.indices.Count;
                this.indices.Add(0b00000000);
            }
            _logger.LogInformation($"Entity {index} created");
            entityCount++;
            return createEntity(index);
        }

        public Int32 GetEntityCount() => entityCount;

        /// <summary>
        /// Increments generation at index specified by id,
        /// effectively destroying the entity.
        /// NOTE: This does not check if the id is valid (generation matches)
        /// if this is necessary use SafeDestroy.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public void Destroy(UInt32 id)
        {
            Int32 index = IdToIndex(id);
            ++this.indices[index];
            this.freeIndices.Enqueue(index);
        }

        /// <summary>
        /// Checks if the id is valid before calling Destroy(id).
        /// See Destroy for more information.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        /// <returns>True if id was valid.</returns>
        public Boolean SafeDestroy(UInt32 id)
        {
            if (!Alive(id))
            {
                return false;
            }

            Destroy(id);
            return true;
        }

        public override String ToString() => "EntityManager";

        /// <summary>
        /// Creates an entity at the given index.
        /// </summary>
        /// <param name="index">Index for new entity</param>
        /// <returns>Reference to created entity.</returns>
        private Entity createEntity(Int32 index)
        {
            UInt32 convertedIndex = (UInt32)index;
            UInt32 generation = (UInt32)this.indices[index] << ENTITY_INDEX_BITS;
            UInt32 id = generation | convertedIndex;
            _logger.LogInformation($"Index:{Convert.ToString(convertedIndex, 2).PadLeft(32, '0')}");
            _logger.LogInformation($"Generation:{Convert.ToString(generation, 2).PadLeft(32, '0')}");
            _logger.LogInformation($"ID:{Convert.ToString(id, 2).PadLeft(32, '0')}");
            return new Entity(id);
        }
    }
}

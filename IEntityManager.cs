using System;
using System.Dynamic;

namespace Simulation
{
    /// <summary>
    /// Interface for managing the creation, maintenance, and destruction
    /// of entities
    /// </summary>
    public interface IEntityManager
    {
        /// <summary>
        /// Check if an entity with the given id is currently alive
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <returns>True if entity exists</returns>
        public Boolean Alive(UInt32 id);

        /// <summary>
        /// Create a new entity with an id according to the
        /// underlying implementation.
        /// </summary>
        /// <returns>Reference to created entity.</returns>
        public Entity Create();

        /// <summary>
        /// Get number of living entities.
        /// </summary>
        /// <returns>Number of living entities.</returns>
        public int GetEntityCount();

        /// <summary>
        /// Destroy the entity with the given id, and perform any cleanup
        /// necessary for the underlying implementation.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public void Destroy(UInt32 id);
    }

    /// <summary>
    /// A wrapper struct for an Entity id
    /// </summary>
    public readonly struct Entity
    {
        public static Entity None = new Entity(UInt32.MaxValue);
        public readonly UInt32 Id;

        public Entity(UInt32 id) => this.Id = id;
    }
}

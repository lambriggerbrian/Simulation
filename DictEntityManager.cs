using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace Simulation
{
    public class DictEntityManager : IEntityManager
    {
        private readonly Dictionary<UInt32, Entity> entities = new Dictionary<UInt32, Entity>();

        public Boolean Alive(UInt32 id) => entities.ContainsKey(id);

        public Entity Create()
        {
            UInt32 id = (UInt32)entities.Count;
            Entity entity = new Entity(id);
            entities.Add((UInt32)entities.Count, entity);
            return entity;
        }

        public void Destroy(UInt32 id) => entities.Remove(id);

        public Int32 GetEntityCount() => entities.Count;

        public override String ToString() => "DictEntityManager";
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Simulation
{
    public class ComponentManagerCollection : ICollection<IComponentManager>
    {
        private readonly Dictionary<string, IComponentManager> nameToManager = new Dictionary<string, IComponentManager>();
        private readonly List<IComponentManager> managers = new List<IComponentManager>();

        private ILogger<ComponentManagerCollection> _logger;

        public ComponentManagerCollection(ILoggerFactory factory) => _logger = factory.CreateLogger<ComponentManagerCollection>();

        public Int32 Count => ((ICollection<IComponentManager>)this.managers).Count;

        public Boolean IsReadOnly => ((ICollection<IComponentManager>)this.managers).IsReadOnly;

        public void AddComponentManager(IComponentManager manager)
        {
            if (nameToManager.ContainsKey(manager.GetName().ToLower()))
            {
                _logger.LogError($"Manager already registered: {manager.GetName()}");
                return;
            }
            managers.Add(manager);
            nameToManager.Add(manager.GetName().ToLower(), manager);
        }

        public IComponentManager GetManagerByName(string name)
        {
            IComponentManager manager;
            nameToManager.TryGetValue(name.ToLower(), out manager);
            if (manager != null) return manager;
            else
            {
                _logger.LogError($"No manager type found: {name}");
                return null;
            }
        }

        public void Add(IComponentManager item) => ((ICollection<IComponentManager>)this.managers).Add(item);

        public void Clear() => ((ICollection<IComponentManager>)this.managers).Clear();

        public Boolean Contains(IComponentManager item) => ((ICollection<IComponentManager>)this.managers).Contains(item);

        public void CopyTo(IComponentManager[] array, Int32 arrayIndex) => ((ICollection<IComponentManager>)this.managers).CopyTo(array, arrayIndex);

        public Boolean Remove(IComponentManager item) => ((ICollection<IComponentManager>)this.managers).Remove(item);

        public IEnumerator<IComponentManager> GetEnumerator() => ((IEnumerable<IComponentManager>)this.managers).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.managers).GetEnumerator();
    }
}

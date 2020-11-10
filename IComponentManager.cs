using System;
using System.Collections.Generic;

namespace Simulation
{
    public interface IComponentManager
    {
        public IEntityManager GetEntityManager();

        public void RegisterComponent(UInt32 id);

        public object GetInstance(UInt32 id);

        public string GetName();
    }

    public interface IComponentManager<S> : IComponentManager where S : struct
    {
        public S GetInstance(int index);

        public bool SetInstance(int index, S data);
    }

    public interface IParentableComponentManager<S, T> where S : IParentable<T>
    {
        public void RegisterComponent(UInt32 id, T parent, List<T> children = null);

        public Boolean AddChild(T parent, T child);
    }

    public interface IMultipleInstanceComponentManager<S> where S : struct
    {
        public List<S> GetInstances(UInt32 id);
    }

    public interface IParentable<T>
    {
        public T Parent { get; set; }
        public T FirstChild { get; set; }
        public T NextSibling { get; set; }
        public T PrevSibling { get; set; }
    }
}

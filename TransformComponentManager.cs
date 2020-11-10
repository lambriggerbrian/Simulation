using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

using Microsoft.Extensions.Logging;

namespace Simulation
{
    public struct TransformData : IParentable<Int32>
    {
        public static readonly TransformData None = new TransformData(Entity.None);
        public readonly UInt32 OwnerId;

        public TransformData(UInt32 ownerId)
        {
            this.OwnerId = ownerId;
            this.LocalTransform = this.WorldTransform = Matrix4x4.Identity;
            this.Parent = -1;
            this.FirstChild = this.NextSibling = this.PrevSibling = -1;
        }

        public TransformData(Entity entity) : this(entity.Id)
        {
        }

        public Matrix4x4 LocalTransform { get; set; }
        public Matrix4x4 WorldTransform { get; set; }
        public Int32 Parent { get; set; }
        public Int32 FirstChild { get; set; }
        public Int32 NextSibling { get; set; }
        public Int32 PrevSibling { get; set; }
        public Vector3 Translation { get => this.WorldTransform.Translation; }

        public static string MatrixToString(Matrix4x4 matrix)
        {
            Quaternion rotation = new Quaternion();
            Vector3 scale = new Vector3();
            Vector3 translation = new Vector3();
            Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);
            return $"Trans:{translation} Rotation:{rotation} Scale:{scale}";
        }

        public override String ToString()
        {
            return $"Transform {OwnerId}: {MatrixToString(WorldTransform)}";
        }
    }

    public class TransformComponentManager : IComponentManager<TransformData>,
        IParentableComponentManager<TransformData, Int32>, IMultipleInstanceComponentManager<TransformData>
    {
        private readonly ILogger<TransformComponentManager> _logger;
        private readonly IEntityManager _manager;

        private readonly Dictionary<UInt32, List<Int32>> entityToInstances = new Dictionary<UInt32, List<Int32>>();
        private readonly Dictionary<Int32, UInt32> instancesToEntities = new Dictionary<Int32, UInt32>();
        private readonly List<Matrix4x4> localTransforms = new List<Matrix4x4>();
        private readonly List<Matrix4x4> worldTransforms = new List<Matrix4x4>();
        private readonly List<Int32> parentInstances = new List<Int32>();
        private readonly List<Int32> firstChildInstances = new List<Int32>();
        private readonly List<Int32> nextSiblingInstances = new List<Int32>();
        private readonly List<Int32> prevSiblingInstances = new List<Int32>();

        public TransformComponentManager()
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Default", LogLevel.Error)
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddConsole();
            });
            _logger = factory.CreateLogger<TransformComponentManager>();
        }

        public TransformComponentManager(ILoggerFactory logger, IEntityManager manager)
        {
            _logger = logger.CreateLogger<TransformComponentManager>();
            _manager = manager;
        }

        public Int32 EntityToInstanceIndex(UInt32 id) => entityToInstances[id][0];

        public List<Int32> EntityToInstanceIndexes(UInt32 id) => entityToInstances[id];

        public IEntityManager GetEntityManager() => _manager;

        public TransformData GetInstance(int index)
        {
            if (!IsValidInstance(index))
            {
                _logger.LogError($"Invalid index");
                return TransformData.None;
            }
            var instance = new TransformData(this.instancesToEntities[index]);
            instance.LocalTransform = this.localTransforms[index];
            instance.WorldTransform = this.worldTransforms[index];
            instance.Parent = this.parentInstances[index];
            instance.FirstChild = this.firstChildInstances[index];
            instance.NextSibling = this.nextSiblingInstances[index];
            instance.PrevSibling = this.prevSiblingInstances[index];
            return instance;
        }

        public List<TransformData> GetInstances(UInt32 id)
        {
            List<Int32> instances;
            entityToInstances.TryGetValue(id, out instances);
            if (instances != null) return instances.Select(i => GetInstance(i)).ToList();
            return new List<TransformData>() { TransformData.None };
        }


        public void RegisterComponent(UInt32 id)
        {
            var index = this.instancesToEntities.Count;
            if (!_manager.Alive(id))
            {
                _logger.LogError($"Cannot register a dead id: {id}");
                return;
            }
            _logger.LogInformation($"Registering component instance {index} for Entity {id}");
            List<int> indexes;
            if (this.entityToInstances.TryGetValue(id, out indexes)) indexes.Add(index);
            else entityToInstances.Add(id, new List<int>() { index });
            this.instancesToEntities.Add(index, id);
            var instance = new TransformData(id);
            AddInstance(instance);
        }

        public void RegisterComponent(UInt32 id, Int32 parent, List<Int32> children = null)
        {
            var index = instancesToEntities.Count;
            RegisterComponent(id);
            AddChild(parent, index);
            if (children != null)
            {
                foreach (var child in children) AddChild(index, child);
            }
        }

        public Boolean AddChild(Int32 parent, Int32 child)
        {
            _logger.LogInformation($"Adding child {child} to parent {parent}");
            // Check that indexes are valid and not circular
            if (!IsValidInstance(parent) || !IsValidInstance(child) || parent == child) return false;
            int parentOfChildIndex = parentInstances[child];
            if (IsValidInstance(parentOfChildIndex)) return false;
            // Set parent and siblings
            parentInstances[child] = parent;
            // Traverse siblings to get last sibling index
            var prevSiblingIndex = firstChildInstances[parent];
            if (!IsValidInstance(prevSiblingIndex)) return true;
            var nextSiblingIndex = nextSiblingInstances[prevSiblingIndex];
            while (nextSiblingIndex > 0)
            {
                prevSiblingIndex = nextSiblingInstances[prevSiblingIndex];
                nextSiblingIndex = nextSiblingInstances[prevSiblingIndex];
            }
            prevSiblingInstances[child] = prevSiblingIndex;
            nextSiblingInstances[prevSiblingIndex] = child;
            return true;
        }

        public Boolean SetInstance(Int32 index, TransformData data)
        {
            if (!IsValidInstance(index)) return false;
            this.localTransforms[index] = data.LocalTransform;
            this.worldTransforms[index] = data.WorldTransform;
            this.parentInstances[index] = data.Parent;
            this.firstChildInstances[index] = data.FirstChild;
            this.nextSiblingInstances[index] = data.NextSibling;
            this.prevSiblingInstances[index] = data.PrevSibling;
            return true;
        }

        public void Translate(int index, Vector3 translation)
        {
            if (worldTransforms.Count <= index)
            {
                _logger.LogError($"Invalid index");
                return;
            }
            _logger.LogInformation($"Translating instance {index} by: {translation}");
            Vector3 sumVector = worldTransforms[index].Translation + translation;
            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(sumVector);
            SetLocalTransform(index, translationMatrix);
        }

        public void SetLocalTransform(int index, Matrix4x4 matrix)
        {
            _logger.LogTrace($"Setting instance {index} local transform.");
            if (!IsValidInstance(index)) return;
            localTransforms[index] = matrix;
            int parentIndex = parentInstances[index];
            Matrix4x4 parentTransform = IsValidInstance(parentIndex) ? worldTransforms[parentIndex] : Matrix4x4.Identity;
            Transform(index, parentTransform);
        }

        public void Transform(int index, Matrix4x4 transformMatrix)
        {
            _logger.LogTrace($"Transforming instance {index}...");
            worldTransforms[index] = localTransforms[index] * transformMatrix;
            Queue<int> childQueue = new Queue<int>();
            int childIndex = firstChildInstances[index];
            while (IsValidInstance(childIndex))
            {
                childQueue.Enqueue(childIndex);
                childIndex = nextSiblingInstances[childIndex];
            }
            foreach (var child in childQueue) Transform(child, worldTransforms[index]);
        }

        public String GetName() => "Transform";

        public Object GetInstance(UInt32 id)
        {
            List<int> instances;
            entityToInstances.TryGetValue(id, out instances);
            if (instances != null) return GetInstance(instances[0]);
            else return null;
        }

        public Type GetDataType() => typeof(TransformData);

        private void AddInstance(TransformData data)
        {
            this.localTransforms.Add(data.LocalTransform);
            this.worldTransforms.Add(data.WorldTransform);
            this.parentInstances.Add(data.Parent);
            this.firstChildInstances.Add(data.FirstChild);
            this.nextSiblingInstances.Add(data.NextSibling);
            this.prevSiblingInstances.Add(data.PrevSibling);
        }

        private Boolean IsValidInstance(int index)
        {
            if (index < 0 || index >= instancesToEntities.Count) return false;
            return true;
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;
using System.Linq;

namespace Rainbow.DomainDriven.Mongo.Internal
{
    public class AggregateRootOperation : IAggregateRootOperation
    {

        private ConcurrentDictionary<Type, ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>> _batchCache = new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>>();

        public void Add(IAggregateRoot root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            var cache = _batchCache.GetOrAdd(root.GetType(), new ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>());

            if (cache.ContainsKey(root.Id))
            {
                throw new Exception($"重复加入实体:{root.GetType().Name} id:{root.Id}");
            }

            cache.TryAdd(root.Id, new RepositoryData<IAggregateRoot>() { State = StoreType.Add, Entity = root });
        }

        public void Clear()
        {
            _batchCache.Clear();
        }

        public IAggregateRoot Get(Type type, Guid id)
        {
            var cache = _batchCache.GetOrAdd(type, new ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>());
            RepositoryData<IAggregateRoot> root;
            if (!cache.TryGetValue(id, out root))
                return null;
            if (root.State == StoreType.Remove)
            {
                throw new Exception($"获取的实体已经被删除:{type.Name} id:{id}");
            }
            return root.Entity;
        }
        private IEnumerable<IAggregateRoot> GetList(Type aggrType, StoreType storeType)
        {
            var cache = _batchCache.GetOrAdd(aggrType, new ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>());
            return cache.Values.Where(a => a.State == storeType).Select(a => a.Entity);
        }

        public IEnumerable<IAggregateRoot> GetAdded(Type aggrType)
        {
            return this.GetList(aggrType, StoreType.Add);
        }

        public Type[] GetAllTypes()
        {
            return _batchCache.Keys.ToArray();
        }

        public IEnumerable<IAggregateRoot> GetRemoved(Type aggrType)
        {
            return this.GetList(aggrType, StoreType.Remove);
        }


        public IEnumerable<IAggregateRoot> GetUpdated(Type aggrType)
        {
            return this.GetList(aggrType, StoreType.Update);
        }

        public void Remove(IAggregateRoot root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            RepositoryData<IAggregateRoot> repoData;
            var cache = _batchCache.GetOrAdd(root.GetType(), new ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>());

            if (!cache.TryGetValue(root.Id, out repoData))
            {
                repoData = new RepositoryData<IAggregateRoot>();
                repoData.Entity = root;
                repoData.State = StoreType.Remove;
                cache.TryAdd(root.Id, repoData);
            }
            if (repoData.State == StoreType.Remove)
            {
                throw new Exception($"更新已删除的实体:{root.GetType().Name} id:{root.Id}");
            }
            repoData.State = StoreType.Remove;
        }

        public void Update(IAggregateRoot root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            RepositoryData<IAggregateRoot> repoData;
            var cache = _batchCache.GetOrAdd(root.GetType(), new ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>());

            if (!cache.TryGetValue(root.Id, out repoData))
            {
                repoData = new RepositoryData<IAggregateRoot>();
                repoData.Entity = root;
                repoData.State = StoreType.Update;
                cache.TryAdd(root.Id, repoData);
            }
            if (repoData.State == StoreType.Remove)
            {
                throw new Exception($"更新已经删除的实体:{root.GetType().Name} id:{root.Id}");
            }

            if (repoData.State == StoreType.Add)
            {
                return;
            }
            repoData.State = StoreType.Update;
        }
    }
}
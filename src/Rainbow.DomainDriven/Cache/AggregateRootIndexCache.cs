using System;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Cache
{
    public class AggregateRootIndexCache : IAggregateRootIndexCache
    {
        private readonly ConcurrentDictionary<Guid, string> _cacheKey;
        private readonly ConcurrentDictionary<string, bool> _cacheIndex;
        public AggregateRootIndexCache()
        {
            this._cacheKey = new ConcurrentDictionary<Guid, string>();
            this._cacheIndex = new ConcurrentDictionary<string, bool>();
        }

        void IAggregateRootIndexCache.Add<TAggregateRoot>(TAggregateRoot root, string key)
        {
            this.Add(root as IAggregateRoot , key);
        }

        void IAggregateRootIndexCache.Remove<TAggregateRoot>(TAggregateRoot root)
        {
            this.Remove(root as IAggregateRoot);
        }

        public void Add(IAggregateRoot root, string key)
        {
            if (this._cacheIndex.ContainsKey(key))
                throw new DomainException(DomainCode.IndexCacheExists.GetHashCode(), $"领域对象{root.GetType().Name}已经包含key:{key}");

            if (!this._cacheIndex.TryAdd(key, true))
                throw new DomainException(DomainCode.IndexCacheExists.GetHashCode(), $"领域对象{root.GetType().Name}已经包含key:{key}");

            _cacheKey.TryAdd(root.Id, key);
        }

        public void Remove(IAggregateRoot root)
        {
            string key;
            bool isIndex;
            if (!_cacheKey.TryRemove(root.Id, out key)) return;
            this._cacheIndex.TryRemove(key, out isIndex);
        }
    }
}
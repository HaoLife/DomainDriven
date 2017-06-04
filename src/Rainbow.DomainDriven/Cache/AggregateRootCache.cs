using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Cache
{
    public class AggregateRootCache : IAggregateRootCache
    {
        private readonly ConcurrentDictionary<string, IAggregateRoot> _cache;
        private readonly HashSet<string> _cacheInvalid;
        private readonly HashSet<string> _cacheUsed;

        public AggregateRootCache()
        {
            this._cache = new ConcurrentDictionary<string, IAggregateRoot>();
            this._cacheInvalid = new HashSet<string>();
            this._cacheUsed = new HashSet<string>();
        }

        private string CacheKey(Type aggregateType, Guid id)
        {
            return $"{aggregateType.Name}:{id.ToShort()}";
        }
        private string CacheKey<TAggregateRoot>(Guid id)
        {
            return this.CacheKey(typeof(TAggregateRoot), id);
        }

        public bool Exists(Type aggregateType, Guid id)
        {
            return this._cache.ContainsKey(this.CacheKey(aggregateType, id));
        }

        public bool Exists(IAggregateRoot aggregate)
        {
            return this._cache.ContainsKey(this.CacheKey(aggregate.GetType(), aggregate.Id));
        }

        public IAggregateRoot Get(Type aggregateType, Guid id)
        {
            IAggregateRoot model;
            var key = this.CacheKey(aggregateType, id);
            if (this._cache.TryGetValue(key, out model))
            {
                this._cacheUsed.Add(key);
            }
            return model;
        }

        public IAggregateRoot Remove(IAggregateRoot aggregate)
        {
            return this.Remove(aggregate.GetType(), aggregate.Id);
        }

        public IAggregateRoot Remove(Type aggregateType, Guid id)
        {
            IAggregateRoot model;
            var key = this.CacheKey(aggregateType, id);
            if (this._cacheUsed.Contains(key)) return null;

            this._cache.TryRemove(key, out model);
            return model;
        }

        public IAggregateRoot Set(IAggregateRoot aggregate)
        {
            this._cache.AddOrUpdate(this.CacheKey(aggregate.GetType(), aggregate.Id), aggregate, (a, b) => aggregate);
            return aggregate;
        }


        bool IAggregateRootCache.Exists<TAggregateRoot>(Guid id)
        {
            return this._cache.ContainsKey(this.CacheKey<TAggregateRoot>(id));
        }

        TAggregateRoot IAggregateRootCache.Get<TAggregateRoot>(Guid id)
        {
            return this.Get(typeof(TAggregateRoot), id) as TAggregateRoot;
        }

        TAggregateRoot IAggregateRootCache.Remove<TAggregateRoot>(Guid id)
        {
            return this.Remove(typeof(TAggregateRoot), id) as TAggregateRoot;
        }

        TAggregateRoot IAggregateRootCache.Set<TAggregateRoot>(TAggregateRoot aggregate)
        {
            this._cache.AddOrUpdate(this.CacheKey<TAggregateRoot>(aggregate.Id), aggregate, (a, b) => aggregate);
            return aggregate;
        }

        public void SetInvalid(Type aggregateType, Guid id)
        {
            this._cacheInvalid.Add(this.CacheKey(aggregateType, id));
        }

        public bool Invalid<TAggregateRoot>(Guid id)
        {
            return this._cacheInvalid.Contains(this.CacheKey(typeof(TAggregateRoot), id));
        }

        public bool Invalid(Type aggregateType, Guid id)
        {
            return this._cacheInvalid.Contains(this.CacheKey(aggregateType, id));
        }

        public void RemoveInvalid(Type aggregateType, Guid id)
        {
            this._cacheInvalid.Remove(this.CacheKey(aggregateType, id));
        }

        public int Used(IEnumerable<IAggregateRoot> aggregates)
        {
            int count = 0;
            foreach (var item in aggregates)
            {
                var key = this.CacheKey(item.GetType(), item.Id);
                if (this._cacheUsed.Remove(key)) count++;
            }
            return count;
        }

        public int Use(IEnumerable<IAggregateRoot> aggregates)
        {
            int count = 0;
            foreach (var item in aggregates)
            {
                var key = this.CacheKey(item.GetType(), item.Id);
                if (this._cacheUsed.Add(key)) count++;
            }
            return count;
        }

        public int Use(Type aggregateType, IEnumerable<Guid> ids)
        {
            int count = 0;
            foreach (var item in ids)
            {
                var key = this.CacheKey(aggregateType, item);
                if (this._cacheUsed.Add(key)) count++;
            }
            return count;
        }

        TAggregateRoot IAggregateRootCache.RemoveWhere<TAggregateRoot>(Guid id, int version)
        {
            return this.RemoveWhere(typeof(TAggregateRoot), id, version) as TAggregateRoot;
        }

        public IAggregateRoot RemoveWhere(IAggregateRoot aggregate)
        {
            return this.RemoveWhere(aggregate.GetType(), aggregate.Id, aggregate.Version);
        }

        public IAggregateRoot RemoveWhere(Type aggregateType, Guid id, int version)
        {
            IAggregateRoot model;
            var key = this.CacheKey(aggregateType, id);
            if (this._cacheUsed.Contains(key)) return null;
            if (!this._cache.TryGetValue(key, out model)) return null;
            if (model.Version != version) return null;

            this._cache.TryRemove(key, out model);
            return model;
        }

    }
}
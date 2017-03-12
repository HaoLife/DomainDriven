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

        public AggregateRootCache()
        {
            this._cache = new ConcurrentDictionary<string, IAggregateRoot>();
            this._cacheInvalid = new HashSet<string>();
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

        public IAggregateRoot Get(IAggregateRoot aggregate, Guid id)
        {
            IAggregateRoot model;
            this._cache.TryGetValue(this.CacheKey(aggregate.GetType(), id), out model);
            return model;
        }

        public IAggregateRoot Remove(IAggregateRoot aggregate)
        {
            IAggregateRoot model;
            this._cache.TryRemove(this.CacheKey(aggregate.GetType(), aggregate.Id), out model);
            return model;
        }

        public IAggregateRoot Remove(Type aggregateType, Guid id)
        {
            IAggregateRoot model;
            this._cache.TryRemove(this.CacheKey(aggregateType, id), out model);
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
            IAggregateRoot model;
            this._cache.TryGetValue(this.CacheKey<TAggregateRoot>(id), out model);
            return model as TAggregateRoot;
        }

        TAggregateRoot IAggregateRootCache.Remove<TAggregateRoot>(Guid id)
        {
            IAggregateRoot model;
            this._cache.TryRemove(this.CacheKey<TAggregateRoot>(id), out model);
            return model as TAggregateRoot;
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
    }
}
using System;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Cache
{
    public class AggregateRootCache : IAggregateRootCache
    {
        private readonly ConcurrentDictionary<string, IAggregateRoot> _cache;

        public AggregateRootCache()
        {
            this._cache = new ConcurrentDictionary<string, IAggregateRoot>();
        }

        private string CacheKey<TAggregateRoot>(Guid id)
        {
            return $"{typeof(TAggregateRoot).Name}:{id.ToShort()}";
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
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingContextCache : IContextCache
    {

        private readonly ConcurrentDictionary<string, IAggregateRoot> _cache = new ConcurrentDictionary<string, IAggregateRoot>();

        private string CacheKey(Type aggregateType, Guid id)
        {
            return $"{aggregateType.Name}:{id.ToString("N")}";
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
            this._cache.TryGetValue(key, out model);
            return model;
        }

        public IAggregateRoot Remove(IAggregateRoot aggregate)
        {
            IAggregateRoot model;
            var key = this.CacheKey(aggregate.GetType(), aggregate.Id);
            this._cache.TryRemove(key, out model);
            return model;
        }

        public IAggregateRoot Set(IAggregateRoot aggregate)
        {
            var key = this.CacheKey(aggregate.GetType(), aggregate.Id);
            this._cache.AddOrUpdate(key, aggregate, (k, v) => aggregate);
            return aggregate;
        }

        bool IContextCache.Exists<TAggregateRoot>(Guid id)
        {
            return this._cache.ContainsKey(this.CacheKey(typeof(TAggregateRoot), id));
        }

        TAggregateRoot IContextCache.Get<TAggregateRoot>(Guid id)
        {
            return this.Get(typeof(TAggregateRoot), id) as TAggregateRoot;
        }

        TAggregateRoot IContextCache.Remove<TAggregateRoot>(Guid id)
        {
            IAggregateRoot model;
            var key = this.CacheKey(typeof(TAggregateRoot), id);
            this._cache.TryRemove(key, out model);
            return model as TAggregateRoot;
        }

        TAggregateRoot IContextCache.Set<TAggregateRoot>(TAggregateRoot aggregate)
        {
            var key = this.CacheKey(typeof(TAggregateRoot), aggregate.Id);
            this._cache.AddOrUpdate(key, aggregate, (k, v) => aggregate);
            return aggregate;
        }

        TAggregateRoot IContextCache.Set<TAggregateRoot>(Guid id, TAggregateRoot aggregate)
        {
            this.Set(typeof(TAggregateRoot), id, aggregate);
            return aggregate;
        }

        public IAggregateRoot Set(Type type, Guid id, IAggregateRoot aggregate)
        {
            var key = this.CacheKey(type, id);
            this._cache.AddOrUpdate(key, aggregate, (k, v) => aggregate);
            return aggregate;
        }
    }
}

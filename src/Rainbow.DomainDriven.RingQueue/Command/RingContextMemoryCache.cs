using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.RingQueue.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingContextMemoryCache : IContextCache
    {
        private IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<Type, HashSet<Guid>> _invalidCache = new ConcurrentDictionary<Type, HashSet<Guid>>();


        public RingContextMemoryCache(IMemoryCache memoryCache)
        {
            this._memoryCache = memoryCache;
        }


        private string CacheKey(Type aggregateType, Guid id)
        {
            return $"{aggregateType.Name}:{id.ToString("N")}";
        }

        public bool Exists(Type aggregateType, Guid id)
        {
            return _memoryCache.TryGetValue(CacheKey(aggregateType, id), out object value);
        }

        public bool Exists(IAggregateRoot aggregate)
        {
            if (aggregate == null) return false;
            return _memoryCache.TryGetValue(CacheKey(aggregate.GetType(), aggregate.Id), out object value);
        }

        public IAggregateRoot Get(Type aggregateType, Guid id)
        {

            object model;
            var key = this.CacheKey(aggregateType, id);
            this._memoryCache.TryGetValue(key, out model);

            if (model is WeakReference reference)
            {
                return reference.Target as IAggregateRoot;
            }

            return model as IAggregateRoot;
        }

        public IAggregateRoot Remove(IAggregateRoot aggregate)
        {
            if (aggregate == null) return null;
            var key = this.CacheKey(aggregate.GetType(), aggregate.Id);
            var result = this._memoryCache.Get<IAggregateRoot>(key);
            this._memoryCache.Remove(key);
            return result;
        }

        public IAggregateRoot Set(IAggregateRoot aggregate)
        {
            if (aggregate == null) return null;
            return this.Set(aggregate.GetType(), aggregate.Id, aggregate);
        }

        public IAggregateRoot Set(Type type, Guid id, IAggregateRoot aggregate)
        {
            var key = this.CacheKey(type, aggregate.Id);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .RegisterPostEvictionCallback(SetWeakReference, null);

            RemoveInvalid(type, id);

            return _memoryCache.Set(key, aggregate, cacheEntryOptions);
        }

        bool IContextCache.Exists<TAggregateRoot>(Guid id)
        {
            return _memoryCache.TryGetValue(CacheKey(typeof(TAggregateRoot), id), out object value);
        }

        TAggregateRoot IContextCache.Get<TAggregateRoot>(Guid id)
        {

            object model;
            var key = this.CacheKey(typeof(TAggregateRoot), id);
            this._memoryCache.TryGetValue(key, out model);

            if (model is WeakReference reference)
            {
                return reference.Target as TAggregateRoot;
            }

            return model as TAggregateRoot;
        }

        TAggregateRoot IContextCache.Remove<TAggregateRoot>(Guid id)
        {
            var key = this.CacheKey(typeof(TAggregateRoot), id);
            var result = this._memoryCache.Get<TAggregateRoot>(key);
            this._memoryCache.Remove(key);
            return result;
        }

        TAggregateRoot IContextCache.Set<TAggregateRoot>(Guid id, TAggregateRoot aggregate)
        {
            this.Set(typeof(TAggregateRoot), id, aggregate);
            return aggregate;

        }

        TAggregateRoot IContextCache.Set<TAggregateRoot>(TAggregateRoot aggregate)
        {
            this.Set(aggregate);
            return aggregate;
        }

        public void SetInvalid(Type aggregateType, Guid id)
        {
            var invalids = _invalidCache.GetOrAdd(aggregateType, (a) => new HashSet<Guid>());
            invalids.Add(id);
        }

        private void RemoveInvalid(Type aggregateType, Guid id)
        {
            var invalids = _invalidCache.GetOrAdd(aggregateType, (a) => new HashSet<Guid>());
            invalids.Remove(id);
        }

        public bool VerifyInvalid(Type aggregateType, Guid id)
        {
            var invalids = _invalidCache.GetOrAdd(aggregateType, (a) => new HashSet<Guid>());
            return invalids.Contains(id);
        }


        private void SetWeakReference(object key, object value, EvictionReason reason, object state)
        {
            if (reason == EvictionReason.Expired)
            {
                _memoryCache.Set(key, new WeakReference(value));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class SnapshootMemoryCache : ISnapshootCache
    {
        private IMemoryCache _memoryCache;

        public SnapshootMemoryCache(IMemoryCache memoryCache)
        {
            this._memoryCache = memoryCache;
        }

        private string CacheKey(Type aggregateType, Guid id)
        {
            return $"ss:{aggregateType.Name}:{id.ToString("N")}";
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
            return _memoryCache.Set(key, aggregate, cacheEntryOptions);
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

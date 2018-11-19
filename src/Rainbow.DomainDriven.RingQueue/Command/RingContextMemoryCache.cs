using Microsoft.Extensions.Caching.Memory;
using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingContextMemoryCache : IContextCache
    {
        private IMemoryCache _memoryCache;
        public RingContextMemoryCache(IMemoryCache memoryCache)
        {
            this._memoryCache = memoryCache;
        }

        public bool Exists(Type aggregateType, Guid id)
        {
            throw new NotImplementedException();
        }

        public bool Exists(IAggregateRoot aggregate)
        {
            throw new NotImplementedException();
        }

        public IAggregateRoot Get(Type aggregateType, Guid id)
        {
            throw new NotImplementedException();
        }

        public IAggregateRoot Remove(IAggregateRoot aggregate)
        {
            throw new NotImplementedException();
        }

        public IAggregateRoot Set(IAggregateRoot aggregate)
        {
            throw new NotImplementedException();
        }

        public IAggregateRoot Set(Type type, Guid id, IAggregateRoot aggregate)
        {
            throw new NotImplementedException();
        }

        bool IContextCache.Exists<TAggregateRoot>(Guid id)
        {
            throw new NotImplementedException();
        }

        TAggregateRoot IContextCache.Get<TAggregateRoot>(Guid id)
        {
            throw new NotImplementedException();
        }

        TAggregateRoot IContextCache.Remove<TAggregateRoot>(Guid id)
        {
            throw new NotImplementedException();
        }

        TAggregateRoot IContextCache.Set<TAggregateRoot>(Guid id, TAggregateRoot aggregate)
        {
            throw new NotImplementedException();
        }

        TAggregateRoot IContextCache.Set<TAggregateRoot>(TAggregateRoot aggregate)
        {
            throw new NotImplementedException();
        }
    }
}

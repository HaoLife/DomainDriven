using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Cache
{
    public interface IAggregateRootCache
    {
        TAggregateRoot Get<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
        TAggregateRoot Set<TAggregateRoot>(TAggregateRoot aggregate) where TAggregateRoot : class, IAggregateRoot;
        TAggregateRoot Remove<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
        bool Exists<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
    }
}

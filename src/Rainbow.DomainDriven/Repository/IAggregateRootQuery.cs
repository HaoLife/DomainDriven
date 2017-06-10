using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootQuery
    {
        TAggregateRoot Get<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
        IEnumerable<TAggregateRoot> Get<TAggregateRoot>(params Guid[] keys) where TAggregateRoot : class, IAggregateRoot;

        IAggregateRoot Get(Type aggregateRootType, Guid id);
        IEnumerable<IAggregateRoot> Get(Type aggregateRootType, params Guid[] keys);
    }
}
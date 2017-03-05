using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootCommonQueryRepository
    {
        TAggregateRoot Get<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
        IEnumerable<TAggregateRoot> Get<TAggregateRoot>(params Guid[] keys) where TAggregateRoot : class, IAggregateRoot;

        IAggregateRoot Get(Type aggregateRootType, Guid id);
        IEnumerable<IAggregateRoot> Get(Type aggregateRootType, params Guid[] keys);
    }
}

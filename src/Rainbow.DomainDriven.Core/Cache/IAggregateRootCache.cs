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


        IAggregateRoot Get(IAggregateRoot aggregate, Guid id);
        IAggregateRoot Set(IAggregateRoot aggregate);
        IAggregateRoot Remove(IAggregateRoot aggregate);
        IAggregateRoot Remove(Type aggregateType, Guid id);
        bool Exists(Type aggregateType, Guid id);
        bool Exists(IAggregateRoot aggregate);

        void SetInvalid(Type aggregateType, Guid id);
        void RemoveInvalid(Type aggregateType, Guid id);
        bool Invalid<TAggregateRoot>(Guid id);
        bool Invalid(Type aggregateType, Guid id);

    }
}

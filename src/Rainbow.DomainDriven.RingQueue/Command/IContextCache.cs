using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public interface IContextCache
    {
        TAggregateRoot Get<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
        TAggregateRoot Set<TAggregateRoot>(Guid id, TAggregateRoot aggregate) where TAggregateRoot : class, IAggregateRoot;
        TAggregateRoot Set<TAggregateRoot>(TAggregateRoot aggregate) where TAggregateRoot : class, IAggregateRoot;
        TAggregateRoot Remove<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
        bool Exists<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;

        IAggregateRoot Get(Type aggregateType, Guid id);
        IAggregateRoot Set(IAggregateRoot aggregate);
        IAggregateRoot Set(Type type, Guid id, IAggregateRoot aggregate);
        IAggregateRoot Remove(IAggregateRoot aggregate);
        bool Exists(Type aggregateType, Guid id);
        bool Exists(IAggregateRoot aggregate);

        void SetInvalid(Type aggregateType, Guid id);
        bool VerifyInvalid(Type aggregateType, Guid id);

    }
}

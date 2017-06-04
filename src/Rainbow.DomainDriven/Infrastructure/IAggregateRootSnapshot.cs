using System;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Infrastructure
{
    public interface IAggregateRootSnapshot
    {
        IAggregateRoot GetSnapshot(Type type, Guid id);

        TAggregateRoot GetSnapshot<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
    }
}
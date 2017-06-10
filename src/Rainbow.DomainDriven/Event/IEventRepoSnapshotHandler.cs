using System;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventRepoSnapshotHandler
    {
        IAggregateRoot GetSnapshot(Type type, Guid id);

        TAggregateRoot GetSnapshot<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
    }
}
using System;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public interface IAggregateRootLockRepositoryProvider
    {
        IAggregateRootLockRepository GetRepo(Type aggregateType);
    }
}
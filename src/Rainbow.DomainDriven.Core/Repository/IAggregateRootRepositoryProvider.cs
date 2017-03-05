using System;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootRepositoryProvider
    {
        IAggregateRootRepository GetRepo(Type aggregateType);
    }
}
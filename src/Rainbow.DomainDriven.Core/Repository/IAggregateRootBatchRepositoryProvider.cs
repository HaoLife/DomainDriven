using System;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootBatchRepositoryProvider
    {
        IAggregateRootBatchRepository GetRepo(Type aggregateType);
    }
}
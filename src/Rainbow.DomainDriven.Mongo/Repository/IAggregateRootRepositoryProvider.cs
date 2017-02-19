using System;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public interface IAggregateRootRepositoryProvider
    {
        IAggregateRootRepository GetAggregateRootRepository(Type aggregateType);
    }
}
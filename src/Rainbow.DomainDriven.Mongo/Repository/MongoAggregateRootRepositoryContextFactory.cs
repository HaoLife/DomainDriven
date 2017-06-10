using System;
using Rainbow.DomainDriven.Mongo.Internal;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoAggregateRootRepositoryContextFactory : IAggregateRootRepositoryContextFactory
    {
        private readonly IMongoDatabaseProvider _mongoDatabaseProvider;
        public MongoAggregateRootRepositoryContextFactory(IMongoDatabaseProvider mongoDatabaseProvider)
        {
            this._mongoDatabaseProvider = mongoDatabaseProvider;
        }

        public IAggregateRootRepositoryContext Create()
        {
            return new MongoAggregateRootRepositoryContext(this._mongoDatabaseProvider, new AggregateRootOperation());
        }
    }
}
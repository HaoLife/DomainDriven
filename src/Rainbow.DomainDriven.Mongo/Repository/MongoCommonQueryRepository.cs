using Rainbow.DomainDriven.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoCommonQueryRepository : IAggregateRootCommonQueryRepository
    {
        private readonly IAggregateRootRepositoryProvider _aggregateRootRepositoryProvider;
        public MongoCommonQueryRepository(IAggregateRootRepositoryProvider aggregateRootRepositoryProvider)
        {
            this._aggregateRootRepositoryProvider = aggregateRootRepositoryProvider;
        }


        IEnumerable<TAggregateRoot> IAggregateRootCommonQueryRepository.Get<TAggregateRoot>(params Guid[] keys)
        {
            return this.Get(typeof(TAggregateRoot), keys).Select(p => p as TAggregateRoot);
        }

        TAggregateRoot IAggregateRootCommonQueryRepository.Get<TAggregateRoot>(Guid id)
        {
            return this.Get(typeof(TAggregateRoot), id) as TAggregateRoot;
        }

        public IAggregateRoot Get(Type aggregateRootType, Guid id)
        {
            return _aggregateRootRepositoryProvider.GetRepo(aggregateRootType).Get(id);
        }

        public IEnumerable<IAggregateRoot> Get(Type aggregateRootType, params Guid[] keys)
        {
            return _aggregateRootRepositoryProvider.GetRepo(aggregateRootType).Get(keys);
        }
    }
}

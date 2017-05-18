using Rainbow.DomainDriven.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.DomainDriven.Domain;
using MongoDB.Driver;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Mongo.Internal;
using Rainbow.DomainDriven.Cache;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoLockAggregateRootRepositoryContext :
        IAggregateRootRepositoryContext
    {
        private readonly IAggregateRootIndexCache _aggregateRootIndexCache;
        private readonly IAggregateRootOperation _aggregateRootOperation;
        
        public MongoLockAggregateRootRepositoryContext(
            IAggregateRootIndexCache aggregateRootIndexCache,
            IAggregateRootOperation aggregateRootOperation)
        {
            this._aggregateRootIndexCache = aggregateRootIndexCache;
            this._aggregateRootOperation = aggregateRootOperation;
        }

        public void Add(IAggregateRoot aggregate)
        {
            this._aggregateRootOperation.Add(aggregate);
        }

        public void Remove(IAggregateRoot aggregate)
        {
            this._aggregateRootOperation.Remove(aggregate);
        }

        public void Update(IAggregateRoot aggregate)
        {
            this._aggregateRootOperation.Update(aggregate);
        }

        public void Commit()
        {
            var unRepo = new ConcurrentDictionary<Type, IAggregateRootRepository>();
            var types = this._aggregateRootOperation.GetAllTypes();
            foreach (var item in types)
            {
                var repo = unRepo.GetOrAdd(item, _aggregateRootLockRepositoryProvider.GetRepo(item));
                repo.Add(this._aggregateRootOperation.GetAdded(item));
            }

            foreach (var item in types)
            {
                var repo = unRepo.GetOrAdd(item, _aggregateRootLockRepositoryProvider.GetRepo(item));
                repo.Update(this._aggregateRootOperation.GetUpdated(item));
                repo.Remove(this._aggregateRootOperation.GetRemoved(item));
            }
            this.UnLock();
            this.RemoveIndex();
            this._aggregateRootOperation.Clear();
        }

        public void RollBack()
        {
            throw new NotImplementedException();
        }
    }
}

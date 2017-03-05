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
        private readonly IAggregateRootLockRepositoryProvider _aggregateRootLockRepositoryProvider;
        private readonly IAggregateRootIndexCache _aggregateRootIndexCache;
        private readonly IAggregateRootOperation _aggregateRootOperation;

        public MongoLockAggregateRootRepositoryContext(
            IAggregateRootLockRepositoryProvider aggregateRootLockRepositoryProvider,
            IAggregateRootIndexCache aggregateRootIndexCache,
            IAggregateRootOperation aggregateRootOperation)
        {
            this._aggregateRootLockRepositoryProvider = aggregateRootLockRepositoryProvider;
            this._aggregateRootIndexCache = aggregateRootIndexCache;
            this._aggregateRootOperation = aggregateRootOperation;
        }

        public void Add(IEnumerable<IAggregateRoot> aggregates)
        {
            foreach (var item in aggregates)
            {
                this.Add(item);
            }
        }

        public void Add(IAggregateRoot aggregate)
        {
            this._aggregateRootOperation.Add(aggregate);
        }


        public void Remove(IEnumerable<IAggregateRoot> aggregates)
        {
            foreach (var item in aggregates)
            {
                this.Remove(item);
            }
        }

        public void Remove(IAggregateRoot aggregate)
        {
            this._aggregateRootOperation.Remove(aggregate);
        }

        public void Update(IEnumerable<IAggregateRoot> aggregates)
        {
            foreach (var item in aggregates)
            {
                this.Update(item);
            }
        }

        public void Update(IAggregateRoot aggregate)
        {
            this._aggregateRootOperation.Update(aggregate);
        }


        private void Lock(long expire)
        {
            try
            {
                var types = this._aggregateRootOperation.GetAllTypes();
                foreach (var item in types)
                {
                    var lockRoots = this._aggregateRootOperation.GetUpdated(item)
                        .Concat(this._aggregateRootOperation.GetRemoved(item));
                    if (!lockRoots.Any()) continue;

                    var repo = _aggregateRootLockRepositoryProvider.GetRepo(item);
                    repo.Lock(lockRoots, expire);

                }
            }
            catch (Exception ex)
            {
                this.UnLock();
                throw ex;
            }
        }
        private void UnLock()
        {
            var types = this._aggregateRootOperation.GetAllTypes();
            foreach (var item in types)
            {
                var lockRoots = this._aggregateRootOperation.GetUpdated(item)
                    .Concat(this._aggregateRootOperation.GetRemoved(item));

                var repo = _aggregateRootLockRepositoryProvider.GetRepo(item);
                repo.UnLock(lockRoots);

            }
        }

        private void RollBackAdd()
        {
            var unRepo = new ConcurrentDictionary<Type, IAggregateRootLockRepository>();

            var types = this._aggregateRootOperation.GetAllTypes();
            foreach (var item in types)
            {
                var adds = this._aggregateRootOperation.GetAdded(item);
                if (!adds.Any()) continue;
                var repo = unRepo.GetOrAdd(item, _aggregateRootLockRepositoryProvider.GetRepo(item));
                repo.Remove(adds);

            }
        }
        private void RemoveIndex()
        {
            var types = this._aggregateRootOperation.GetAllTypes();
            foreach (var item in types)
            {
                var adds = this._aggregateRootOperation.GetAdded(item);
                if (!adds.Any()) continue;

                foreach (var obj in adds)
                {
                    this._aggregateRootIndexCache.Remove(obj);
                }
            }
        }

        public void RollBack()
        {
            this.UnLock();
            this.RemoveIndex();
            this.RollBackAdd();
            this._aggregateRootOperation.Clear();
        }

        public void Commit()
        {
            //1.锁住变更对象
            //2.执行操作
            //3.解锁
            var expire = DateTime.Now.AddSeconds(10).Ticks;
            //锁住领域对象
            this.Lock(expire);
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

    }
}

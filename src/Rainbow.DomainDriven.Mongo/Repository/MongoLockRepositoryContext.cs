using Rainbow.DomainDriven.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.DomainDriven.Domain;
using MongoDB.Driver;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Mongo.Internal;
using System.Net;
using System.Net.Sockets;
using Rainbow.DomainDriven.Cache;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoLockAggregateRootRepositoryContext :
        IAggregateRootRepositoryContext
    {
        private readonly IAggregateRootRepositoryProvider _aggregateRootRepositoryProvider;
        private readonly IAggregateRootIndexCache _aggregateRootIndexCache;
        private string _IpAddress;

        private ConcurrentDictionary<Type, ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>> _batchCache = new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>>();

        public MongoLockAggregateRootRepositoryContext(
            IAggregateRootRepositoryProvider aggregateRootRepositoryProvider,
            IAggregateRootIndexCache aggregateRootIndexCache)
        {
            this._aggregateRootRepositoryProvider = aggregateRootRepositoryProvider;
            this._aggregateRootIndexCache = aggregateRootIndexCache;
            this.Init();
        }

        private void Init()
        {
            var task = Dns.GetHostAddressesAsync(Environment.MachineName);
            task.Wait();
            var ip = task.Result.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            this._IpAddress = string.Empty;
            if (ip != null)
            {
                this._IpAddress = ip.ToString();
            }
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
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));
            var cache = _batchCache.GetOrAdd(aggregate.GetType(), new ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>());

            if (cache.ContainsKey(aggregate.Id))
            {
                throw new Exception($"重复加入实体:{aggregate.GetType().Name} id:{aggregate.Id}");
            }

            cache.TryAdd(aggregate.Id, new RepositoryData<IAggregateRoot>() { State = StoreType.Add, Entity = aggregate });
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
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));
            RepositoryData<IAggregateRoot> repoData;
            var cache = _batchCache.GetOrAdd(aggregate.GetType(), new ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>());

            if (!cache.TryGetValue(aggregate.Id, out repoData))
            {
                repoData = new RepositoryData<IAggregateRoot>();
                repoData.Entity = aggregate;
                repoData.State = StoreType.Remove;
                cache.TryAdd(aggregate.Id, repoData);
            }
            if (repoData.State == StoreType.Remove)
            {
                throw new Exception($"更新已删除的实体:{aggregate.GetType().Name} id:{aggregate.Id}");
            }
            repoData.State = StoreType.Remove;
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
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));
            RepositoryData<IAggregateRoot> repoData;
            var cache = _batchCache.GetOrAdd(aggregate.GetType(), new ConcurrentDictionary<Guid, RepositoryData<IAggregateRoot>>());

            if (!cache.TryGetValue(aggregate.Id, out repoData))
            {
                repoData = new RepositoryData<IAggregateRoot>();
                repoData.Entity = aggregate;
                repoData.State = StoreType.Update;
                cache.TryAdd(aggregate.Id, repoData);
            }
            if (repoData.State == StoreType.Remove)
            {
                throw new Exception($"更新已经删除的实体:{aggregate.GetType().Name} id:{aggregate.Id}");
            }

            if (repoData.State == StoreType.Add)
            {
                return;
            }
            repoData.State = StoreType.Update;
        }


        private void Lock(RowLock rowLock)
        {
            foreach (var item in _batchCache)
            {
                var lockRoots = item.Value.Where(a => a.Value.State != StoreType.Add).Select(p => p.Value.Entity);
                if (!lockRoots.Any()) break;

                var repo = _aggregateRootRepositoryProvider.GetAggregateRootRepository(item.Key);
                repo.Lock(rowLock, lockRoots);
            }
        }
        private void UnLock()
        {
            foreach (var item in _batchCache)
            {
                var lockRoots = item.Value.Where(a => a.Value.State != StoreType.Add).Select(p => p.Value.Entity);
                if (!lockRoots.Any()) break;

                var repo = _aggregateRootRepositoryProvider.GetAggregateRootRepository(item.Key);
                repo.UnLock(lockRoots);
            }
        }

        private void RollBackAdd()
        {
            foreach (var item in _batchCache)
            {
                var adds = item.Value.Values.Where(a => a.State == StoreType.Add).ToList();
                if (!adds.Any()) break;

                var repo = _aggregateRootRepositoryProvider.GetAggregateRootRepository(item.Key);
                foreach (var obj in adds)
                {
                    repo.Remove(obj.Entity);
                }
            }

        }
        private void RemoveIndex()
        {
            
            foreach (var item in _batchCache)
            {
                var adds = item.Value.Values.Where(a => a.State == StoreType.Add).ToList();
                if (!adds.Any()) break;

                foreach (var obj in adds)
                {
                    this._aggregateRootIndexCache.Remove(obj.Entity);
                }
            }
        }

        public void RollBack()
        {
            this.UnLock();
            this.RemoveIndex();
            this.RollBackAdd();

            _batchCache.Clear();
        }

        public void Commit()
        {
            //1.锁住变更对象
            //2.先创建对象
            //3.更新对象
            //4.解锁

            var rowLock = new RowLock(_IpAddress, DateTime.Now.AddSeconds(10).Ticks);
            //锁住领域对象
            this.Lock(rowLock);

            //先创建对象
            foreach (var item in _batchCache)
            {
                var adds = item.Value.Values.Where(a => a.State == StoreType.Add).ToList();
                if (!adds.Any()) break;

                var repo = _aggregateRootRepositoryProvider.GetAggregateRootRepository(item.Key);
                foreach (var obj in adds)
                {
                    repo.Add(obj.Entity);
                }
            }

            //更新领域对象
            foreach (var item in _batchCache)
            {
                var changes = item.Value.Values.Where(a => a.State == StoreType.Update || a.State == StoreType.Remove).ToList();
                if (!changes.Any()) break;

                var repo = _aggregateRootRepositoryProvider.GetAggregateRootRepository(item.Key);
                foreach (var a in changes)
                {
                    switch (a.State)
                    {
                        case StoreType.Update:
                            repo.Update(a.Entity);
                            break;
                        case StoreType.Remove:
                            repo.Remove(a.Entity);
                            break;
                    }
                }
            }
            this.UnLock();
            this.RemoveIndex();
            _batchCache.Clear();

        }

    }
}

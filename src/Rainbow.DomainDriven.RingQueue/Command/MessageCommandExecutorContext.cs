using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class MessageCommandExecutorContext : ICommandExecutorContext
    {
        private readonly List<IAggregateRoot> _unNoticeRoots;
        private readonly IAggregateRootQuery _commonRepository;
        private readonly IAggregateRootCache _aggregateRootCache;
        public IEnumerable<IAggregateRoot> TrackedAggregateRoots => this._unNoticeRoots;

        public MessageCommandExecutorContext(
            IAggregateRootQuery commonRepository
            , IAggregateRootCache aggregateRootCache
            )
        {
            this._commonRepository = commonRepository;
            this._aggregateRootCache = aggregateRootCache;
            this._unNoticeRoots = new List<IAggregateRoot>();
        }

        public void Clear()
        {
            this._aggregateRootCache.Used(this._unNoticeRoots);
            this._unNoticeRoots.ForEach(p => p.Clear());
            this._unNoticeRoots.Clear();
        }

        void ICommandContext.Add<TAggregateRoot>(TAggregateRoot aggregate)
        {
            if (this._unNoticeRoots.Exists(p => p.Id == aggregate.Id))
            {
                throw new DomainException(DomainCode.AggregateCacheExists.GetHashCode());
            }

            this._aggregateRootCache.Set(aggregate);
            this._unNoticeRoots.Add(aggregate);
        }

        TAggregateRoot ICommandContext.Get<TAggregateRoot>(Guid id)
        {
            var aggregate = default(TAggregateRoot);
            var isAddCache = false;
            aggregate = this._unNoticeRoots.Find(p => p.Id == id) as TAggregateRoot;

            if (aggregate == null)
            {
                if (this._aggregateRootCache.Invalid<TAggregateRoot>(id))
                    throw new NullReferenceException("没有找到该对象");
                aggregate = this._aggregateRootCache.Get<TAggregateRoot>(id);
            }

            if (aggregate == null)
            {
                aggregate = this._commonRepository.Get<TAggregateRoot>(id);
                isAddCache = true;
            }

            if (aggregate == null) return null;

            this._unNoticeRoots.Add(aggregate);
            if (isAddCache)
            {
                this._aggregateRootCache.Set(aggregate);
            }
            return aggregate;
        }


    }
}
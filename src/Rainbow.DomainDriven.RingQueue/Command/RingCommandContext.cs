using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingCommandContext : ICommandContext
    {

        private List<IAggregateRoot> _unNoticeRoots = new List<IAggregateRoot>();
        private IAggregateRootRebuilder _aggregateRootRebuilder;
        private IContextCache _contextCache;
        public IEnumerable<IAggregateRoot> TrackedAggregateRoots => this._unNoticeRoots;

        public RingCommandContext(IContextCache contextCache, IAggregateRootRebuilder aggregateRootRebuilder)
        {
            this._contextCache = contextCache;
            this._aggregateRootRebuilder = aggregateRootRebuilder;
        }

        public void Clear()
        {
            this._unNoticeRoots.ForEach(p => p.Clear());
            this._unNoticeRoots.Clear();
        }


        void ICommandContext.Add<TAggregateRoot>(TAggregateRoot aggregate)
        {
            if (this._unNoticeRoots.Exists(p => p.Id == aggregate.Id))
            {
                throw new DomainException(DomainCode.AggregateExists);
            }

            this._unNoticeRoots.Add(aggregate);
        }

        TAggregateRoot ICommandContext.Get<TAggregateRoot>(Guid id)
        {
            var aggregate = default(TAggregateRoot);
            aggregate = this._unNoticeRoots.Find(p => p.Id == id) as TAggregateRoot;

            if (aggregate == null && this._contextCache.Exists<TAggregateRoot>(id))
            {
                aggregate = this._contextCache.Get<TAggregateRoot>(id);
            }

            if (aggregate == null)
            {
                aggregate = this._aggregateRootRebuilder.Rebuild<TAggregateRoot>(id);
                this._contextCache.Set<TAggregateRoot>(id, aggregate);
            }

            if (aggregate == null) return null;

            this._unNoticeRoots.Add(aggregate);
            return aggregate;
        }
    }
}

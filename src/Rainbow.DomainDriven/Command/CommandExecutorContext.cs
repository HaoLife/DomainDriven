using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Command
{
    public class CommandExecutorContext : ICommandExecutorContext
    {
        private readonly List<IAggregateRoot> _unNoticeRoots;
        private readonly IAggregateRootQuery _aggregateRootQuery;
        public IEnumerable<IAggregateRoot> TrackedAggregateRoots => this._unNoticeRoots;

        public CommandExecutorContext(
            IAggregateRootQuery aggregateRootQuery
            )
        {
            this._aggregateRootQuery = aggregateRootQuery;
            this._unNoticeRoots = new List<IAggregateRoot>();
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
                throw new DomainException(DomainCode.AggregateCacheExists.GetHashCode());
            }

            this._unNoticeRoots.Add(aggregate);
        }

        TAggregateRoot ICommandContext.Get<TAggregateRoot>(Guid id)
        {
            var aggregate = default(TAggregateRoot);
            aggregate = this._unNoticeRoots.Find(p => p.Id == id) as TAggregateRoot;

            if (aggregate == null)
            {
                aggregate = this._aggregateRootQuery.Get<TAggregateRoot>(id);
            }

            if (aggregate == null) return null;

            this._unNoticeRoots.Add(aggregate);
            return aggregate;
        }


    }
}
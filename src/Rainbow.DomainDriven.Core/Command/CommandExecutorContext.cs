using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Command
{
    public sealed class CommandExecutorContext : ICommandExecutorContext
    {
        private readonly List<IAggregateRoot> _unNoticeRoots;
        private readonly IAggregateRootQuery _commonRepository;
        public IEnumerable<IAggregateRoot> TrackedAggregateRoots => this._unNoticeRoots;

        public CommandExecutorContext(
            IAggregateRootQuery commonRepository
            )
        {
            this._commonRepository = commonRepository;
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
                aggregate = this._commonRepository.Get<TAggregateRoot>(id);
            }

            if (aggregate == null) return null;

            this._unNoticeRoots.Add(aggregate);
            return aggregate;
        }


    }
}

using System;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Infrastructure
{
    public class AggregateRootSnapshot : IAggregateRootSnapshot
    {
        private readonly IAggregateRootCommonQuery _aggregateRootCommonQuery;
        private readonly IEventSourceRepository _eventSourceRepository;

        public AggregateRootSnapshot(
            IAggregateRootCommonQuery aggregateRootCommonQuery
            , IEventSourceRepository eventSourceRepository)
        {
            this._aggregateRootCommonQuery = aggregateRootCommonQuery;
            this._eventSourceRepository = eventSourceRepository;
        }
        public IAggregateRoot GetSnapshot(Type type, Guid id)
        {
            var root = this._aggregateRootCommonQuery.Get(type, id);
            var evts = this._eventSourceRepository.GetAggregateRootEvents(id, type.Name, root.Version);
            foreach (var item in evts)
            {
                root.ReplayEvent(item.Event);
            }
            return root;
        }

        public TAggregateRoot GetSnapshot<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot
        {
            var root = this.GetSnapshot(typeof(TAggregateRoot), id);
            return root as TAggregateRoot;
        }
    }
}
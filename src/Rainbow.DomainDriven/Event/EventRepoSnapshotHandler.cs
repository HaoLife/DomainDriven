using System;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Event
{
    public class EventRepoSnapshotHandler : IEventRepoSnapshotHandler
    {
        private readonly IAggregateRootQuery _aggregateRootQuery;
        private readonly IEventSourceRepository _eventSourceRepository;
        private readonly IEventReplayHandler _replayEventHandler;

        public EventRepoSnapshotHandler(
            IAggregateRootQuery aggregateRootQuery
            , IEventSourceRepository eventSourceRepository
            , IEventReplayHandler replayEventHandler)
        {
            this._aggregateRootQuery = aggregateRootQuery;
            this._eventSourceRepository = eventSourceRepository;
            this._replayEventHandler = replayEventHandler;
        }
        public IAggregateRoot GetSnapshot(Type type, Guid id)
        {
            var root = this._aggregateRootQuery.Get(type, id);
            var evts = this._eventSourceRepository.GetAggregateRootEvents(id, type.Name, root.Version);
            foreach (var item in evts)
            {
                this._replayEventHandler.Handle(root, item.Event);
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
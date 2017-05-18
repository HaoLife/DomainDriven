using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Repository
{
    public interface IEventSourceRepository
    {
        void AddRange(IEnumerable<EventSource> events);

        IEnumerable<EventSource> GetAggregateRootEvents(Guid aggregateRootId, string aggregateRootTypeName, int version = 0);
    }
}
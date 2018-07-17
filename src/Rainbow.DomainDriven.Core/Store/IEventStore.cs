using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Store
{
    public interface IEventStore
    {
        void AddRange(IEnumerable<IEvent> events);

        IEnumerable<IEvent> GetAggregateRootEvents(Guid aggregateRootId, string aggregateRootTypeName, int version = 0);

        List<IEvent> Take(int size, long uTCTimestamp = 0);
    }
}

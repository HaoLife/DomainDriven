using System.Collections.Generic;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Repository
{
    public interface IEventSourceRepository
    {
        void AddRange(IEnumerable<DomainEventSource> events);
    }
}
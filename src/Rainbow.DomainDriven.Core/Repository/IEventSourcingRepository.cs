using System.Collections.Generic;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Repository
{
    public interface IEventSourcingRepository
    {
        void Save(DomainEventSource current);
        DomainEventSource Current();

        List<DomainEventSource> Take(DomainEventSource current, int size);
    }
}
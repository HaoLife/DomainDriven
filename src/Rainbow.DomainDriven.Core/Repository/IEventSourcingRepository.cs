using System.Collections.Generic;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Repository
{
    public interface IEventSourcingRepository
    {
        void Save(EventSource current);
        EventSource Current();

        List<EventSource> Take(EventSource current, int size);
    }
}
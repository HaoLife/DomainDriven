using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Repository
{
    public interface IEventSourcingRepository
    {
         
        void Save(EventSource current);
        EventSource Current();

    }
}
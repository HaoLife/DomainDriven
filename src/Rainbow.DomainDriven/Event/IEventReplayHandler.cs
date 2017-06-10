using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventReplayHandler
    {
        void Handle(IAggregateRoot root, IEvent evt);
    }
}
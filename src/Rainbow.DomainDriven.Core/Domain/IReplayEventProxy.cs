using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Domain
{
    public interface IReplayEventProxy
    {
        void Handle(IAggregateRoot root, IEvent evt);
    }
}
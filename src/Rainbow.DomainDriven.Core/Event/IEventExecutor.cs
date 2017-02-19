using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventExecutor
    {
        void Handle(DomainMessage<DomainEventStream> message);
    }
}
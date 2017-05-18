using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventService
    {
        void Publish(DomainMessage<EventStream> message);
    }
}
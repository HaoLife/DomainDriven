namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandler
    {
        void Handle(IEvent evt);
    }
}
namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandlerProxy
    {
         void Handle(IEvent evt);
    }
}
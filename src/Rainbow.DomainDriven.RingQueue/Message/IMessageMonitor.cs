namespace Rainbow.DomainDriven.RingQueue.Message
{
    public interface IMessageMonitor
    {
         void Wait(string key);
    }
}
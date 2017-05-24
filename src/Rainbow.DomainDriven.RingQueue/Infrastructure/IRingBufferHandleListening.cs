namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public interface IRingBufferHandleListening
    {
         void Wait(long sequence);
    }
}
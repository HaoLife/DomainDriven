namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public interface IQueueConsumer
    {
         
        Sequence Sequence { get; }

        void Run();
    }
}
namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public interface IQueueHandler<TMessage>
    {
        void Handle(TMessage message, long sequence, bool isEnd);
    }
}
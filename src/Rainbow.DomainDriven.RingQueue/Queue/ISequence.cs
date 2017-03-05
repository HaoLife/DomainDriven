namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public interface ISequence
    {
        long Value { get; }
        void SetValue(long value);
        bool CompareAndSet(long expectedSequence, long nextSequence);
    }
}
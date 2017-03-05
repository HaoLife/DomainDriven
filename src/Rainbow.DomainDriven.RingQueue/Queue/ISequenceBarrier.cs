namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public interface ISequenceBarrier
    {
        long WaitFor(long sequence);

        long Cursor { get; }
    }
}
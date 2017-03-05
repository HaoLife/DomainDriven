namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public interface ISequencer
    {
        int BufferSize { get; }
        long Next();
        long Next(int n);
        void Publish(long sequence);
        void Publish(long lo, long hi);
        long GetUseSequence(long lo, long hi);
        void AddGatingSequences(params ISequence[] gatingSequences);
        ISequenceBarrier NewBarrier(params ISequence[] sequencesToTrack);
    }
}
using System.Threading;

namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public class Sequence: ISequence
    {
        public const long InitialCursorValue = -1;
        private Fields _fields;

        public Sequence(long initialValue = InitialCursorValue)
        {
            _fields = new Fields(initialValue);
        }

        public long Value => _fields.Value;

        public bool CompareAndSet(long expectedSequence, long nextSequence)
        {
            return Interlocked.CompareExchange(ref _fields.Value, nextSequence, expectedSequence) == expectedSequence;
        }

        public void SetValue(long value)
        {
            _fields.Value = value;
        }

        private struct Fields
        {
            public long Value;

            public Fields(long value)
            {
                Value = value;
            }
        }
    }
}
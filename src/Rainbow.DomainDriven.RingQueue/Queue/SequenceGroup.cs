using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.RingQueue.Utilities;

namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public class SequenceGroup : ISequence
    {
        private readonly IEnumerable<ISequence> _sequences;

        public SequenceGroup(IEnumerable<ISequence> sequences)
        {
            this._sequences = sequences;
        }

        public long Value => RingUtil.GetMinimum(_sequences);

        public bool CompareAndSet(long expectedSequence, long nextSequence)
        {
            throw new NotSupportedException();
        }

        public void SetValue(long value)
        {
            throw new NotSupportedException();
        }
    }
}
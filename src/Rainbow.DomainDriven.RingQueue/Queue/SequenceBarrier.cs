using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public class SequenceBarrier : ISequenceBarrier
    {

        private readonly ISequencer _sequencer;
        private readonly ISequence _cursor;
        private readonly ISequence _dependent;


        public SequenceBarrier(ISequencer sequencer, ISequence cursor, IEnumerable<ISequence> dependents)
        {
            this._sequencer = sequencer;
            this._cursor = cursor;
            this._dependent = !dependents.Any() ? cursor : new SequenceGroup(dependents);
        }

        public long Cursor => _dependent.Value;

        public long WaitFor(long sequence)
        {
            var spinWait = default(SpinWait);

            while (_dependent.Value < sequence)
            {
                spinWait.SpinOnce();
            }
            return _sequencer.GetUseSequence(sequence, _dependent.Value);
        }
    }
}
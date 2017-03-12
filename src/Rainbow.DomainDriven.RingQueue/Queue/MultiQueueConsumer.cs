using System;
using System.Threading;

namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public class MultiQueueConsumer<TMessage> : IQueueConsumer
    {

        private volatile int _running;
        private readonly IRingQueue<TMessage>[] _messageQueues;
        private readonly ISequenceBarrier[] _sequenceBarriers;
        private readonly IQueueHandler<TMessage> _messageHandler;
        private readonly Sequence[] _sequences;
        private long _count;

        public MultiQueueConsumer(
            IRingQueue<TMessage>[] messageQueues,
            ISequenceBarrier[] sequenceBarriers,
            IQueueHandler<TMessage> messageHandler)
        {
            this._messageQueues = messageQueues;
            this._sequenceBarriers = sequenceBarriers;
            this._messageHandler = messageHandler;

            _sequences = new Sequence[messageQueues.Length];
            for (var i = 0; i < _sequences.Length; i++)
            {
                _sequences[i] = new Sequence();
            }
        }


        public Sequence Sequence => throw new NotSupportedException();

        public Sequence[] GetSequences()
        {
            return _sequences;
        }


        public void Run()
        { 
            if (Interlocked.Exchange(ref _running, 1) != 0)
                throw new InvalidOperationException("Thread is already running");


            var barrierLength = _sequenceBarriers.Length;

            while (true)
            {
                try
                {
                    for (var i = 0; i < barrierLength; i++)
                    {
                        var available = _sequenceBarriers[i].WaitFor(-1);
                        var sequence = _sequences[i];

                        var nextSequence = sequence.Value + 1;

                        for (var l = nextSequence; l <= available; l++)
                        {
                            _messageHandler.Handle(_messageQueues[i][l].Value, l, nextSequence == available);
                        }

                        sequence.SetValue(available);

                        _count += available - nextSequence + 1;
                    }
                }
                catch (TimeoutException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }
        }
    }
}
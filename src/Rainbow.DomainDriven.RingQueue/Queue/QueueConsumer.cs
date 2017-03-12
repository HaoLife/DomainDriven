using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public class QueueConsumer<TMessage> : IQueueConsumer
    {
        private volatile int _running;
        private readonly IRingQueue<TMessage> _messageQueue;
        private readonly ISequenceBarrier _sequenceBarrier;
        private readonly IQueueHandler<TMessage> _messageHandler;
        private Sequence _sequence = new Sequence();

        public QueueConsumer(
            IRingQueue<TMessage> messageQueue,
            ISequenceBarrier sequenceBarrier,
            IQueueHandler<TMessage> messageHandler)
        {
            this._messageQueue = messageQueue;
            this._sequenceBarrier = sequenceBarrier;
            this._messageHandler = messageHandler;
        }

        public Sequence Sequence => _sequence;

        public void Run()
        {
            if (Interlocked.Exchange(ref _running, 1) != 0)
            {
                throw new InvalidOperationException("Thread is already running");
            }

            while (true)
            {
                try
                {
                    var nextSequence = _sequence.Value + 1L;
                    var availableSequence = _sequenceBarrier.WaitFor(nextSequence);
                    while (nextSequence <= availableSequence)
                    {
                        var evt = _messageQueue[nextSequence];
                        _messageHandler.Handle(evt.Value, nextSequence, nextSequence == availableSequence);
                        nextSequence++;
                    }
                    _sequence.SetValue(availableSequence);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
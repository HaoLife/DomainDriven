using System;
using System.Threading;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class RingBufferHandleListening<TMessage> : IRingBufferHandleListening
    {
        private readonly IRingBuffer<TMessage> _ringQueue;
        public RingBufferHandleListening(IRingBuffer<TMessage> ringQueue)
        {
            this._ringQueue = ringQueue;
        }

        public void Wait(long sequence)
        {
            SpinWait wait = new SpinWait();
            while (!this._ringQueue.IsUsed(sequence))
            {
                wait.SpinOnce();
            }
        }
    }
}
using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public abstract class AbstractBatchMessageHandler<TMessage> :
         IEventHandler<WrapMessage<TMessage>>
    {
        private static int defaultMaxHandleCount = 5000;

        private List<TMessage> _temps;
        private int maxHandleCount;
        public AbstractBatchMessageHandler()
            : this(defaultMaxHandleCount)
        {

        }
        public AbstractBatchMessageHandler(int maxHandleCount)
        {
            this.maxHandleCount = maxHandleCount;
            this._temps = new List<TMessage>(this.maxHandleCount);
        }


        public abstract void Handle(TMessage[] messages, long endSequence);


        public void OnEvent(TMessage data, long sequence, bool endOfBatch)
        {
        }

        public void OnEvent(WrapMessage<TMessage> data, long sequence, bool endOfBatch)
        {
            _temps.Add(data.Value);
            if (endOfBatch || _temps.Count >= maxHandleCount)
            {
                this.Handle(_temps.ToArray(), sequence);
                _temps.Clear();
            }
        }
    }
}
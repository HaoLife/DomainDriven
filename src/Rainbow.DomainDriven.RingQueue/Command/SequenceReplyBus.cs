using Rainbow.MessageQueue.Ring;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class SequenceReplyBus : IReplyBus
    {
        private RingBuffer<ReplyMessage> _replyQueue;

        public SequenceReplyBus(RingBuffer<ReplyMessage> replyQueue)
        {
            this._replyQueue = replyQueue;

        }

        public void Publish(ReplyMessage[] events)
        {
            var seq = _replyQueue.Next(events.Length);
            var index = seq - events.Length;
            while (index <= seq)
            {
                _replyQueue.Publish(index);
                index++;
            }
        }
    }
}

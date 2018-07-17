using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public interface IReplyBus
    {
        void Publish(ReplyMessage[] events);
    }
}

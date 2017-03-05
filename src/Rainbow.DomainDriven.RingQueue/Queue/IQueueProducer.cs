using System.Collections.Generic;

namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public interface IQueueProducer<TMessage>
    {
        long Send(TMessage message);
        long Send(IEnumerable<TMessage> messages);
    }
}
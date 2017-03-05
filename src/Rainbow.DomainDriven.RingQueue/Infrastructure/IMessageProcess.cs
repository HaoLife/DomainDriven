using System.Collections.Generic;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Queue;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public interface IMessageProcess
    {
        IRingQueue<DomainMessage> GetQueue(string queueName);
        Dictionary<string, IQueueConsumer> GetConsumers(string queueName);
        IQueueConsumer GetConsumer(string queueName, string consumer);

        void Start();
    }
}
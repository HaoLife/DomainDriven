using System.Collections.Generic;
using Rainbow.DomainDriven.Message;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public interface IMessageProcess
    {
        IRingBuffer<DomainMessage> GetQueue(string queueName);
        IEnumerable<IRingBuffer<DomainMessage>> GetQueues(string queueName);
        Dictionary<string, IRingBufferConsumer> GetConsumers(string queueName);
        IRingBufferConsumer GetConsumer(string queueName, string consumer);

        void Start();
    }
}
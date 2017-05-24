using System.Collections.Generic;
using Rainbow.DomainDriven.Message;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public interface IMessageProcess
    {
        IRingBuffer<TMessage> GetQueue<TMessage>(string queueName);
        IEnumerable<IRingBuffer<TMessage>> GetQueues<TMessage>(string queueName);
        IEnumerable<KeyValuePair<string, IRingBufferConsumer>> GetConsumers(string queueName);
        IRingBufferConsumer GetConsumer(string queueName, string consumerName);

        void Start();
        void Halt();
    }
}
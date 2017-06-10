using System.Collections.Generic;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Message
{
    public interface IMessageProcess
    {
        void AddQueue<TMessage>(string queueName, IRingBuffer<TMessage> queue);
        void AddConsumer(string consumerName, IRingBufferConsumer messageConsumer);

        IRingBuffer<TMessage> GetQueue<TMessage>(string queueName);
        IEnumerable<IRingBuffer<TMessage>> GetQueues<TMessage>(string queueName);
        IRingBufferConsumer GetConsumer(string consumerName);

        void Start();
        void Halt();
    }
}
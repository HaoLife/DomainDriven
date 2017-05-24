using Rainbow.DomainDriven.Message;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public interface IMessageProcessBuilder
    {
        void AddQueue<TMessage>(string queueName, IRingBuffer<TMessage> queue);
        void AddConsumer(string queueName,string consumerName,IRingBufferConsumer messageConsumer);
        IMessageProcess Build();

    }
}
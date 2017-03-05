using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Queue;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public interface IMessageProcessBuilder
    {
        void AddQueue(string queueName, IRingQueue<DomainMessage> queue);

        void AddConsumer(string queueName, string consumerName, IQueueConsumer messageConsumer);
        IMessageProcess Build();
    }
}
using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Queue;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class MessageProcessBuilder : IMessageProcessBuilder
    {
        private readonly Dictionary<string, IRingQueue<DomainMessage>> _queue;
        //todo: modify by haozi 这种可以优化，可以采用.net core IConfiguration 的方法进行优化
        private readonly Dictionary<string, Dictionary<string, IQueueConsumer>> _messageConsumer;

        public MessageProcessBuilder()
        {
            this._queue = new Dictionary<string, IRingQueue<DomainMessage>>();
            this._messageConsumer = new Dictionary<string, Dictionary<string, IQueueConsumer>>();
        }

        public void AddConsumer(string queueName, string consumerName, IQueueConsumer messageConsumer)
        {
            Dictionary<string, IQueueConsumer> value;
            if (!this._messageConsumer.TryGetValue(queueName, out value))
            {
                value = new Dictionary<string, IQueueConsumer>();
                this._messageConsumer.Add(queueName, value);
            }
            value.Add(consumerName, messageConsumer);
        }

        public void AddQueue(string queueName, IRingQueue<DomainMessage> queue)
        {
            this._queue.Add(queueName, queue);
        }

        public IMessageProcess Build()
        {
            return new MessageProcess(this._queue, this._messageConsumer);
        }
    }
}
using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Message;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class MessageProcessBuilder : IMessageProcessBuilder
    {
        private readonly Dictionary<string, IRingBuffer<DomainMessage>> _queue;
        //todo: modify by haozi 这种可以优化，可以采用.net core IConfiguration 的方法进行优化
        private readonly Dictionary<string, Dictionary<string, IRingBufferConsumer>> _messageConsumer;

        public MessageProcessBuilder()
        {
            this._queue = new Dictionary<string, IRingBuffer<DomainMessage>>();
            this._messageConsumer = new Dictionary<string, Dictionary<string, IRingBufferConsumer>>();
        }

        public void AddConsumer(string queueName, string consumerName, IRingBufferConsumer messageConsumer)
        {
            Dictionary<string, IRingBufferConsumer> value;
            if (!this._messageConsumer.TryGetValue(queueName, out value))
            {
                value = new Dictionary<string, IRingBufferConsumer>();
                this._messageConsumer.Add(queueName, value);
            }
            value.Add(consumerName, messageConsumer);
        }

        public void AddQueue(string queueName, IRingBuffer<DomainMessage> queue)
        {
            this._queue.Add(queueName, queue);
        }

        public IMessageProcess Build()
        {
            return new MessageProcess(this._queue, this._messageConsumer);
        }
    }
}
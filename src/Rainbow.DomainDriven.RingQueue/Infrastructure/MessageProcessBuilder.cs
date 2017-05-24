using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Message;
using Rainbow.MessageQueue.Ring;
using System.Linq;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class MessageProcessBuilder : IMessageProcessBuilder
    {
        private readonly List<KeyValuePair<string, object>> _queues;
        private readonly Dictionary<string, IRingBufferConsumer> _messageConsumers;

        public MessageProcessBuilder()
        {
            this._queues = new List<KeyValuePair<string, object>>();
            this._messageConsumers = new Dictionary<string, IRingBufferConsumer>();
        }

        public void AddConsumer(string queueName, string consumerName, IRingBufferConsumer messageConsumer)
        {
            var name = $"{queueName}:{consumerName}";
            if (this._messageConsumers.ContainsKey(queueName))
            {
                throw new Exception("已经存在该对象不能重复进行添加");
            }
            this._messageConsumers.Add(name, messageConsumer);
        }

        public void AddQueue<TMessage>(string queueName, IRingBuffer<TMessage> queue)
        {
            var kv = new KeyValuePair<string, object>(queueName, queue);
            this._queues.Add(kv);
        }

        public IMessageProcess Build()
        {
            return new MessageProcess(this._queues, this._messageConsumers);
        }
    }
}
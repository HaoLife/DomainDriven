using System;
using System.Collections.Generic;
using Rainbow.MessageQueue.Ring;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.RingQueue.Message
{
    public class MessageProcess : IMessageProcess
    {
        private readonly List<KeyValuePair<string, object>> _queues;
        private readonly Dictionary<string, IRingBufferConsumer> _messageConsumers;
        public MessageProcess()
        {
            this._queues = new List<KeyValuePair<string, object>>();
            this._messageConsumers = new Dictionary<string, IRingBufferConsumer>();
        }

        public void AddConsumer(string consumerName, IRingBufferConsumer messageConsumer)
        {
            if (this._messageConsumers.ContainsKey(consumerName))
            {
                throw new Exception("已经存在该对象不能重复进行添加");
            }
            this._messageConsumers.Add(consumerName, messageConsumer);
        }

        public void AddQueue<TMessage>(string queueName, IRingBuffer<TMessage> queue)
        {
            var kv = new KeyValuePair<string, object>(queueName, queue);
            this._queues.Add(kv);
        }

        public IRingBufferConsumer GetConsumer(string consumerName)
        {
            this._messageConsumers.TryGetValue(consumerName, out IRingBufferConsumer value);
            return value;
        }

        public IRingBuffer<TMessage> GetQueue<TMessage>(string queueName)
        {
            return this.GetQueues<TMessage>(queueName).FirstOrDefault();
        }

        public IEnumerable<IRingBuffer<TMessage>> GetQueues<TMessage>(string queueName)
        {
            foreach (var item in this._queues)
            {
                if (item.Key.Contains(queueName) && item.Value is IRingBuffer<TMessage>)
                    yield return item.Value as IRingBuffer<TMessage>;
            }
        }

        public void Start()
        {
            var each = this._messageConsumers.Select(a => a.Value);
            foreach (var item in each)
            {
                Task.Factory.StartNew(a => ((IRingBufferConsumer)a).Run(), item, TaskCreationOptions.LongRunning);
            }
        }

        public void Halt()
        {
            var each = this._messageConsumers.Select(a => a.Value);
            foreach (var item in each)
            {
                item.Halt();
            }
        }
    }
}
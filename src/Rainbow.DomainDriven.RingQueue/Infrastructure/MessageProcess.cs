using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Message;
using System.Linq;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class MessageProcess : IMessageProcess
    {
        private readonly Dictionary<string, IRingBuffer<DomainMessage>> _queue;
        private readonly Dictionary<string, Dictionary<string, IRingBufferConsumer>> _messageConsumer;
        public MessageProcess(
            Dictionary<string, IRingBuffer<DomainMessage>> queue
            , Dictionary<string, Dictionary<string, IRingBufferConsumer>> messageConsumer
            )
        {
            this._queue = queue;
            this._messageConsumer = messageConsumer;
        }

        public Dictionary<string, IRingBufferConsumer> GetConsumers(string queueName)
        {
            Dictionary<string, IRingBufferConsumer> value;
            if (!this._messageConsumer.TryGetValue(queueName, out value))
                new Dictionary<string, IRingBufferConsumer>();
            return value;
        }

        public IRingBufferConsumer GetConsumer(string queueName, string consumer)
        {
            var values = this.GetConsumers(queueName);
            IRingBufferConsumer value;
            values.TryGetValue(consumer, out value);
            return value;
        }

        public IRingBuffer<DomainMessage> GetQueue(string queueName)
        {
            IRingBuffer<DomainMessage> value;
            this._queue.TryGetValue(queueName, out value);
            return value;
        }

        public void Start()
        {
            var each = this._messageConsumer.Values.SelectMany(a => a.Values);
            foreach (var item in each)
            {
                Task.Factory.StartNew(a => ((IRingBufferConsumer)a).Run(), item);
            }
        }

        public IEnumerable<IRingBuffer<DomainMessage>> GetQueues(string queueName)
        {
            foreach (var item in this._queue)
            {
                if (item.Key.Contains(queueName)) 
                    yield return item.Value;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Message;
using System.Linq;
using Rainbow.DomainDriven.RingQueue.Queue;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class MessageProcess : IMessageProcess
    {
        private readonly Dictionary<string, IRingQueue<DomainMessage>> _queue;
        private readonly Dictionary<string, Dictionary<string, IQueueConsumer>> _messageConsumer;
        public MessageProcess(
            Dictionary<string, IRingQueue<DomainMessage>> queue
            , Dictionary<string, Dictionary<string, IQueueConsumer>> messageConsumer
            )
        {
            this._queue = queue;
            this._messageConsumer = messageConsumer;
        }

        public Dictionary<string, IQueueConsumer> GetConsumers(string queueName)
        {
            Dictionary<string, IQueueConsumer> value;
            if (!this._messageConsumer.TryGetValue(queueName, out value))
                new Dictionary<string, IQueueConsumer>();
            return value;
        }

        public IQueueConsumer GetConsumer(string queueName, string consumer)
        {
            var values = this.GetConsumers(queueName);
            IQueueConsumer value;
            values.TryGetValue(consumer, out value);
            return value;
        }

        public IRingQueue<DomainMessage> GetQueue(string queueName)
        {
            IRingQueue<DomainMessage> value;
            this._queue.TryGetValue(queueName, out value);
            return value;
        }

        public void Start()
        {
            var each = this._messageConsumer.Values.SelectMany(a => a.Values);
            foreach (var item in each)
            {
                Task.Factory.StartNew(a => ((IQueueConsumer)a).Run(), item);
            }
        }

        public IEnumerable<IRingQueue<DomainMessage>> GetQueues(string queueName)
        {
            foreach (var item in this._queue)
            {
                if (item.Key.Contains(queueName)) 
                    yield return item.Value;
            }
        }
    }
}
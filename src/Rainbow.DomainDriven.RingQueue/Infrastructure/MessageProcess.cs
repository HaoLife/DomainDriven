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
        private readonly IEnumerable<KeyValuePair<string, object>> _queues;
        private readonly IEnumerable<KeyValuePair<string, IRingBufferConsumer>> _messageConsumers;
        public MessageProcess(
            IEnumerable<KeyValuePair<string, object>> queues
            , IEnumerable<KeyValuePair<string, IRingBufferConsumer>> messageConsumers
            )
        {
            this._queues = queues;
            this._messageConsumers = messageConsumers;
        }

        public IRingBuffer<TMessage> GetQueue<TMessage>(string queueName)
        {
            return this.GetQueues<TMessage>(queueName).First();
        }

        public IEnumerable<IRingBuffer<TMessage>> GetQueues<TMessage>(string queueName)
        {
            foreach (var item in this._queues)
            {
                if (item.Key.Contains(queueName) && item.Value is IRingBuffer<TMessage>)
                    yield return item.Value as IRingBuffer<TMessage>;
            }
        }

        IEnumerable<KeyValuePair<string, IRingBufferConsumer>> IMessageProcess.GetConsumers(string queueName)
        {
            foreach (var item in this._messageConsumers)
            {
                if (item.Key.StartsWith(queueName))
                    yield return item;
            }
        }

        public IRingBufferConsumer GetConsumer(string queueName, string consumerName)
        {
            var name = $"{queueName}:{consumerName}";
            foreach (var item in this._messageConsumers)
            {
                if (item.Key.StartsWith(name))
                {
                    return item.Value;
                }
            }
            throw new NullReferenceException("没找到该对象");
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
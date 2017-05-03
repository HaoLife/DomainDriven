using System;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Utilities;
using System.Linq;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingMultiQueueEventService : ICommandService
    {
        private readonly IRingBufferProducer<DomainMessage>[] _messageProducers;
        private readonly IMessageListening _messageListening;
        public RingMultiQueueEventService(
            IMessageProcessBuilder messageProcessBuilder,
            IMessageListening messageListening
            )
        {
            this._messageListening = messageListening;
            var process = messageProcessBuilder.Build();
            var queues = process.GetQueues(QueueName.CommandQueue).ToArray();
            _messageProducers = new IRingBufferProducer<DomainMessage>[queues.Length];
            for (int i = 0; i < _messageProducers.Length; i++)
                _messageProducers[i] = new RingBufferProducer<DomainMessage>(queues[i]);
        }

        public void Publish<TCommand>(DomainMessage cmd) where TCommand : class
        {
            var index = cmd.Head.Priority.GetHashCode();
            this._messageProducers[index].Send(cmd);
            
            if (!string.IsNullOrEmpty(cmd.Head.ReplyKey))
            {
                var message = this._messageListening.WiatFor(cmd.Head.ReplyKey);
                if (!message.IsSuccess) throw message.Exception;
            }
        }
    }
}
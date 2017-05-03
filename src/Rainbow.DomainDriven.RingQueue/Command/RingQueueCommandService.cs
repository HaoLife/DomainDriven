using System;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Utilities;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingQueueCommandService : ICommandService
    {
        private readonly IRingBufferProducer<DomainMessage> _messageProducer;
        private readonly IMessageListening _messageListening;
        public RingQueueCommandService(
            IMessageProcessBuilder messageProcessBuilder,
            IMessageListening messageListening
            )
        {
            this._messageListening = messageListening;
            var process = messageProcessBuilder.Build();
            var queue = process.GetQueue(QueueName.CommandQueue);
            this._messageProducer = new RingBufferProducer<DomainMessage>(queue);
        }

        public void Publish<TCommand>(DomainMessage cmd) where TCommand : class
        {
            this._messageProducer.Send(cmd);
            if (!string.IsNullOrEmpty(cmd.Head.ReplyKey))
            {
                var message = this._messageListening.WiatFor(cmd.Head.ReplyKey);
                if (!message.IsSuccess) throw message.Exception;
            }
        }
    }
}
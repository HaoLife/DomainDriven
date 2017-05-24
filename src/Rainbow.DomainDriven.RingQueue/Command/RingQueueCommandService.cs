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
        private readonly IRingBufferProducer<DomainMessage<ICommand>> _messageProducer;
        private readonly IReplyMessageListening _messageListening;
        private readonly IRingBufferHandleListening _ringBufferHandleListening;

        private const string COMMAND_QUEUE_NAME = QueueName.CommandQueue;

        public RingQueueCommandService(
            IMessageProcessBuilder messageProcessBuilder,
            IReplyMessageListening messageListening
            )
        {
            this._messageListening = messageListening;
            var process = messageProcessBuilder.Build();
            var queue = process.GetQueue<DomainMessage<ICommand>>(COMMAND_QUEUE_NAME);
            this._messageProducer = new RingBufferProducer<DomainMessage<ICommand>>(queue);
            this._ringBufferHandleListening = new RingBufferHandleListening<DomainMessage<ICommand>>(queue);
        }

        public void Publish(DomainMessage<ICommand> message)
        {
            var seq = this._messageProducer.Send(message);
            Wait(seq, message.Head);
        }

        protected virtual void Wait(long sequence, MessageHead head)
        {
            ReplyMessage reply = null;
            switch (head.Consistency)
            {
                case Consistency.Finally:
                    this._ringBufferHandleListening.Wait(sequence);
                    this._messageListening.TryGet(head.ReplyTo, out reply);
                    break;
                case Consistency.Strong:
                    this._ringBufferHandleListening.Wait(sequence);
                    reply = this._messageListening.ForWait(head.ReplyTo);
                    break;
            }
            if (reply != null && !reply.IsSuccess) throw reply.Exception;
        }
    }
}
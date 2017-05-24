using System;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Utilities;
using System.Linq;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingMultiQueueCommandService : ICommandService
    {
        private readonly IRingBufferProducer<DomainMessage<ICommand>>[] _messageProducers;
        private readonly IReplyMessageListening _messageListening;
        private readonly IRingBufferHandleListening[] _ringBufferHandleListenings;

        private const string COMMAND_QUEUE_NAME = QueueName.CommandQueue;

        public RingMultiQueueCommandService(
            IMessageProcessBuilder messageProcessBuilder,
            IReplyMessageListening messageListening)
        {

            this._messageListening = messageListening;
            var process = messageProcessBuilder.Build();
            var queues = process.GetQueues<DomainMessage<ICommand>>(COMMAND_QUEUE_NAME).ToArray();
            this._messageProducers = new IRingBufferProducer<DomainMessage<ICommand>>[queues.Length];
            this._ringBufferHandleListenings = new RingBufferHandleListening<DomainMessage<ICommand>>[queues.Length];

            for (int i = 0; i < queues.Length; i++)
            {
                this._messageProducers[i] = new RingBufferProducer<DomainMessage<ICommand>>(queues[i]);
                this._ringBufferHandleListenings[i] = new RingBufferHandleListening<DomainMessage<ICommand>>(queues[i]);
            }

        }

        public void Publish(DomainMessage<ICommand> message)
        {
            var index = message.Head.Priority.GetHashCode();
            var seq = this._messageProducers[index].Send(message);
            Wait(index, seq, message.Head);
        }

        protected virtual void Wait(int index, long sequence, MessageHead head)
        {
            ReplyMessage reply = null;
            switch (head.Consistency)
            {
                case Consistency.Finally:
                    this._ringBufferHandleListenings[index].Wait(sequence);
                    this._messageListening.TryGet(head.ReplyTo, out reply);
                    break;
                case Consistency.Strong:
                    this._ringBufferHandleListenings[index].Wait(sequence);
                    reply = this._messageListening.ForWait(head.ReplyTo);
                    break;
            }
            if (reply != null && !reply.IsSuccess) throw reply.Exception;
        }
    }
}
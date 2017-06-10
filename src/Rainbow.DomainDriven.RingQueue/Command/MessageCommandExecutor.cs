using System;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class MessageCommandExecutor : ICommandExecutor
    {
        private readonly IMessageMonitor _messageMonitor;
        private readonly MessageDescribe _messageDescribe;
        private readonly IRingBufferProducer<DomainMessage<ICommand>> _messageProducer;
        private bool IsReply;
        public MessageCommandExecutor(
            IMessageMonitor messageMonitor,
            MessageDescribe messageDescribe,
            IRingBufferProducer<DomainMessage<ICommand>> messageProducer)
        {
            this._messageMonitor = messageMonitor;
            this._messageDescribe = messageDescribe;
            this._messageProducer = messageProducer;
            this.SetIsReply();

        }
        public void Handle(ICommand command)
        {
            var message = BuildDommainMessage(command);
            this._messageProducer.Send(message);
            this._messageMonitor.Wait(message.ReplyKey);

        }

        protected virtual DomainMessage<ICommand> BuildDommainMessage(ICommand command)
        {
            var domainMessage = new DomainMessage<ICommand>()
            {
                MessageDescribe = this._messageDescribe,
                Content = command,
            };
            if (this.IsReply)
            {
                domainMessage.ReplyKey = Guid.NewGuid().ToString("N");
            }
            return domainMessage;
        }

        protected virtual void SetIsReply()
        {
            if (this._messageDescribe.Consistency != Consistency.Lose) this.IsReply = true;
        }
    }
}
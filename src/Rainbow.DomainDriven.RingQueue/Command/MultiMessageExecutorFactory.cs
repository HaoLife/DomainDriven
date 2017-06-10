using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.DomainDriven.RingQueue.Utilities;
using Rainbow.MessageQueue.Ring;
using System.Linq;
using Rainbow.DomainDriven.Command;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class MultiMessageExecutorFactory : MessageExecutorFactory
    {
        public MultiMessageExecutorFactory(IMessageProcess messageProcess, IReplyMessageStore replyMessageStore)
            : base(messageProcess, replyMessageStore)
        {
        }


        protected override IRingBufferProducer<DomainMessage<ICommand>> BuildBufferProducer(MessageDescribe key)
        {
            var commandQueues = this._messageProcess.GetQueues<DomainMessage<ICommand>>(QueueName.CommandQueue);
            var commandQueue = commandQueues.ToArray()[key.Priority.GetHashCode()];
            return new RingBufferProducer<DomainMessage<ICommand>>(commandQueue);
        }

    }
}
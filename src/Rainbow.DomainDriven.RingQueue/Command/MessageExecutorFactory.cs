using System;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.DomainDriven.RingQueue.Utilities;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class MessageExecutorFactory : ICommandExecutorFactory
    {
        protected readonly IMessageProcess _messageProcess;
        protected readonly IReplyMessageStore _replyMessageStore;
        private ConcurrentDictionary<MessageDescribe, ICommandExecutor> _cacheCommandExecutor = new ConcurrentDictionary<MessageDescribe, ICommandExecutor>();
        public MessageExecutorFactory(IMessageProcess messageProcess, IReplyMessageStore replyMessageStore)
        {
            this._messageProcess = messageProcess;
            this._replyMessageStore = replyMessageStore;
        }

        public ICommandExecutor Create(MessageDescribe describe)
        {
            return _cacheCommandExecutor.GetOrAdd(describe, Build);
        }

        private ICommandExecutor Build(MessageDescribe key)
        {
            IMessageMonitor messageMonitor = BuildMessageMonitor(key);
            IRingBufferProducer<DomainMessage<ICommand>> messageProducer = BuildBufferProducer(key);
            var executor = new MessageCommandExecutor(messageMonitor, key, messageProducer);

            return executor;
        }

        protected virtual IMessageMonitor BuildMessageMonitor(MessageDescribe key)
        {

            switch (key.Consistency)
            {
                case Consistency.Lose:
                    return new NullMessageMonitor();
                default:
                    return new MessageMonitor(this._replyMessageStore);
            }
        }

        protected virtual IRingBufferProducer<DomainMessage<ICommand>> BuildBufferProducer(MessageDescribe key)
        {
            var commandQueue = this._messageProcess.GetQueue<DomainMessage<ICommand>>(QueueName.CommandQueue);
            return new RingBufferProducer<DomainMessage<ICommand>>(commandQueue);
        }
        
    }
}
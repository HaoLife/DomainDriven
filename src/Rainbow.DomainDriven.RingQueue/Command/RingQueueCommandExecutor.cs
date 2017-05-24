using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;
using System.Linq;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Repository;
using Rainbow.MessageQueue.Ring;
using Rainbow.DomainDriven.RingQueue.Utilities;

namespace Rainbow.DomainDriven.RingQueue.Command
{

    public class RingQueueCommandExecutor : ICommandExecutor
    {
        private readonly ICommandExecutorContext _commandExecutorContext;
        private readonly IAggregateRootSnapshot _aggregateRootSnapshot;
        private readonly IAggregateRootCache _aggregateRootCache;
        private readonly ICommandHandlerProxy _commandHandlerProxy;
        private readonly IEventSourceRepository _eventSourceRepository;
        private readonly RingBufferProducer<DomainMessage<EventStream>> _ringBufferProducer;
        private readonly IReplyMessageListening _replyMessageListening;
        private const string EVENT_NAME_QUEUE = QueueName.EventQueue;

        public RingQueueCommandExecutor(
            ICommandHandlerProxy commandHandlerProxy
            , ICommandExecutorContextFactory commandExecutorContextFactory
            , IAggregateRootSnapshot aggregateRootSnapshot
            , IAggregateRootCache aggregateRootCache
            , IEventSourceRepository eventSourceRepository
            , IMessageProcess messageProcess
            , IReplyMessageListening replyMessageListening
            )
        {
            this._commandHandlerProxy = commandHandlerProxy;
            this._commandExecutorContext = commandExecutorContextFactory.Create();
            this._aggregateRootSnapshot = aggregateRootSnapshot;
            this._aggregateRootCache = aggregateRootCache;
            this._eventSourceRepository = eventSourceRepository;
            this._replyMessageListening = replyMessageListening;
            var eventQueue = messageProcess.GetQueue<DomainMessage<EventStream>>(EVENT_NAME_QUEUE);
            var producer = new RingBufferProducer<DomainMessage<EventStream>>(eventQueue);
        }

        protected virtual DomainMessage<EventStream> BuildEventMessage(MessageHead head, IEnumerable<IAggregateRoot> roots)
        {
            var events = roots
                .SelectMany(p => p.UncommittedEvents.Select(item => new EventSource(item, p.GetType().Name, p.Id)));

            var stream = new EventStream(events.ToList());
            var eventHead = new MessageHead(head.ReplyTo, Priority.Normal, head.Consistency);
            return new DomainMessage<EventStream>(eventHead, stream);
        }

        protected virtual bool CheckNotify(MessageHead head)
        {
            return Consistency.Lose != head.Consistency && !string.IsNullOrEmpty(head.ReplyTo);
        }

        protected virtual IEnumerable<IAggregateRoot> GetSnapshots(ICommandExecutorContext context, IAggregateRootSnapshot aggregateRootSnapshot)
        {
            var evts = context.TrackedAggregateRoots
                .SelectMany(p => p.UncommittedEvents);

            //回溯领域对象并设置到缓存中
            if (evts.Any())
            {
                foreach (var root in context.TrackedAggregateRoots)
                {
                    var sourceRoot = aggregateRootSnapshot.GetSnapshot(root.GetType(), root.Id);
                    yield return sourceRoot;
                }
            }
        }

        protected virtual void Rollback(IEnumerable<IAggregateRoot> roots)
        {
            foreach (var root in roots)
                this._aggregateRootCache.Set(root);
        }

        protected virtual ReplyMessage BuildReplyMessage(string replyKey, Exception ex = null)
        {
            var replyMessage = new ReplyMessage() { IsSuccess = ex != null, Exception = ex, ReplyKey = replyKey };
            return replyMessage;
        }

        protected virtual void StoreEvents(IEnumerable<EventSource> eventSources)
        {
            this._eventSourceRepository.AddRange(eventSources);
        }

        protected virtual void Notify(List<ReplyMessage> replyMessages)
        {
            foreach (var item in replyMessages)
                this._replyMessageListening.Add(item);
        }

        public void Handle(params DomainMessage<ICommand>[] messages)
        {
            List<DomainMessage<EventStream>> evtMessages = new List<DomainMessage<EventStream>>();
            //异常消息通知
            List<ReplyMessage> replyMessages = new List<ReplyMessage>();
            foreach (var message in messages)
            {
                try
                {
                    this._commandHandlerProxy.Handle(this._commandExecutorContext, message.Content);
                    //获取变更的事件
                    var evtMessage = this.BuildEventMessage(message.Head, this._commandExecutorContext.TrackedAggregateRoots);
                    evtMessages.Add(evtMessage);

                }
                catch (Exception ex)
                {
                    //通知消息
                    if (CheckNotify(message.Head))
                    {
                        var replyMessage = this.BuildReplyMessage(message.Head.ReplyTo, ex);
                        replyMessages.Add(replyMessage);
                    }

                    var sourceRoots = this.GetSnapshots(this._commandExecutorContext, this._aggregateRootSnapshot);
                    this.Rollback(sourceRoots);
                }
                finally
                {
                    this._commandExecutorContext.Clear();
                }
            }
            //事件源存储，事件消息堆积，消息通知
            var sources = evtMessages.SelectMany(a => a.Content.Sources);
            this.StoreEvents(sources);
            this._ringBufferProducer.Send(evtMessages);
            //通知消息
            this.Notify(replyMessages);
        }

    }
}
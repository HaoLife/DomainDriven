using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.MessageQueue.Ring;
using System.Linq;
using Rainbow.DomainDriven.RingQueue.Utilities;
using Rainbow.DomainDriven.Cache;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class MessageCommandExecutorHandler : IMessageHandler<DomainMessage<ICommand>>
    {

        private readonly ICommandExecutorContext _commandExecutorContext;
        private readonly IEventRepoSnapshotHandler _eventRepoSnapshotHandler;
        private readonly IAggregateRootCache _aggregateRootCache;
        private readonly ICommandHandler _commandHandler;
        private readonly IEventSourceRepository _eventSourceRepository;
        private readonly IRingBufferProducer<DomainMessage<EventStream>> _ringBufferProducer;
        private readonly IReplyMessageStore _replyMessageStore;
        private readonly IEventReplayHandler _replayEventHandler;
        private const string EVENT_NAME_QUEUE = QueueName.EventQueue;

        public MessageCommandExecutorHandler(
            ICommandHandler commandHandler
            , ICommandExecutorContextFactory commandExecutorContextFactory
            , IEventRepoSnapshotHandler eventRepoSnapshotHandler
            , IAggregateRootCache aggregateRootCache
            , IEventSourceRepository eventSourceRepository
            , IMessageProcess messageProcess
            , IReplyMessageStore replyMessageStore
            , IEventReplayHandler replayEventHandler
            )
        {
            this._commandHandler = commandHandler;
            this._commandExecutorContext = commandExecutorContextFactory.Create();
            this._eventRepoSnapshotHandler = eventRepoSnapshotHandler;
            this._aggregateRootCache = aggregateRootCache;
            this._eventSourceRepository = eventSourceRepository;
            this._replyMessageStore = replyMessageStore;
            this._replayEventHandler = replayEventHandler;
            var eventQueue = messageProcess.GetQueue<DomainMessage<EventStream>>(EVENT_NAME_QUEUE);
            this._ringBufferProducer = new RingBufferProducer<DomainMessage<EventStream>>(eventQueue);

        }

        protected virtual void PushReply(List<ReplyMessage> replyMessages, DomainMessage<ICommand> message, Exception ex = null)
        {
            if (!CheckNotify(message)) return;

            var replyMessage = this.BuildReplyMessage(message.ReplyKey, ex);
            replyMessages.Add(replyMessage);
        }

        protected virtual DomainMessage<EventStream> BuildEventMessage(DomainMessage<ICommand> message, IEnumerable<IAggregateRoot> roots)
        {
            var events = roots
                .SelectMany(p => p.UncommittedEvents.Select(item => new EventSource(item, p.GetType().Name, p.Id)));

            var stream = new EventStream(events.ToList());
            string replyKey = null;
            if (message.MessageDescribe.Consistency == Consistency.Strong)
            {
                replyKey = message.ReplyKey;
            }
            return new DomainMessage<EventStream>() { ReplyKey = replyKey, Content = stream, MessageDescribe = message.MessageDescribe };
        }

        protected virtual bool CheckNotify(DomainMessage<ICommand> message)
        {
            return Consistency.Finally == message.MessageDescribe.Consistency && !string.IsNullOrEmpty(message.ReplyKey);
        }

        protected virtual IEnumerable<IAggregateRoot> GetSnapshots(ICommandExecutorContext context, IEventRepoSnapshotHandler snapshotHandler, List<DomainMessage<EventStream>> evtMessages)
        {
            var evts = context.TrackedAggregateRoots
                .SelectMany(p => p.UncommittedEvents);

            //回溯领域对象并设置到缓存中
            if (evts.Any())
            {

                var currentEvents = evtMessages.SelectMany(a => a.Content.Sources);
                foreach (var root in context.TrackedAggregateRoots)
                {
                    var sourceRoot = snapshotHandler.GetSnapshot(root.GetType(), root.Id);
                    var cevts = currentEvents.Where(a => a.AggregateRootId == sourceRoot.Id && a.AggregateRootTypeName == sourceRoot.GetType().Name);
                    foreach (var e in cevts)
                    {
                        this._replayEventHandler.Handle(sourceRoot, e.Event);
                    }
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
                this._replyMessageStore.Add(item);
        }

        public void Handle(DomainMessage<ICommand>[] messages)
        {

            List<DomainMessage<EventStream>> evtMessages = new List<DomainMessage<EventStream>>();
            //异常消息通知
            List<ReplyMessage> replyMessages = new List<ReplyMessage>();
            foreach (var message in messages)
            {
                try
                {
                    this._commandHandler.Handle(this._commandExecutorContext, message.Content);
                    //获取变更的事件
                    var evtMessage = this.BuildEventMessage(message, this._commandExecutorContext.TrackedAggregateRoots);
                    evtMessages.Add(evtMessage);
                    this.PushReply(replyMessages, message);

                }
                catch (Exception ex)
                {
                    //通知消息
                    this.PushReply(replyMessages, message, ex);
                    var sourceRoots = this.GetSnapshots(this._commandExecutorContext, this._eventRepoSnapshotHandler, evtMessages);

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
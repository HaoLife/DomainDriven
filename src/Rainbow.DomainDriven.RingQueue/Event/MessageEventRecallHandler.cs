using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Infrastructure;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Cache;
using Rainbow.MessageQueue.Ring;
using Rainbow.DomainDriven.RingQueue.Message;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class MessageEventRecallHandler : IMessageHandler<DomainMessage<EventStream>>
    {
        private readonly IEventReplayHandler _replayEventHandler;
        private readonly IAggregateRootQuery _aggregateRootQuery;
        private readonly IDomainTypeProvider _domainTypeProvider;
        private readonly IReplyMessageStore _replyMessageStore;
        private readonly IAggregateRootRepositoryContext _aggregateRootRepositoryContext;
        private readonly ILogger _logger;


        public MessageEventRecallHandler(
            IAggregateRootQuery aggregateRootQuery
            , IDomainTypeProvider domainTypeProvider
            , IReplyMessageStore replyMessageStore
            , IAggregateRootCache aggregateRootCache
            , IAggregateRootRepositoryContextFactory aggregateRootRepositoryContextFactory
            , ILoggerFactory loggerFactory)
        {
            this._replayEventHandler = new EventReplayHandler();
            this._aggregateRootQuery = aggregateRootQuery;
            this._domainTypeProvider = domainTypeProvider;
            this._replyMessageStore = replyMessageStore;
            this._aggregateRootRepositoryContext = aggregateRootRepositoryContextFactory.Create();
            this._logger = loggerFactory.CreateLogger<MessageEventRecallHandler>();
        }

        protected virtual bool CheckNotify(DomainMessage<EventStream> message)
        {
            return Consistency.Strong == message.MessageDescribe.Consistency && !string.IsNullOrEmpty(message.ReplyKey);
        }


        protected virtual ReplyMessage BuildReplyMessage(string replyKey, Exception ex = null)
        {
            var replyMessage = new ReplyMessage() { IsSuccess = ex == null, Exception = ex, ReplyKey = replyKey };
            return replyMessage;
        }


        protected virtual void Notify(List<ReplyMessage> replyMessages)
        {
            foreach (var item in replyMessages)
                this._replyMessageStore.Add(item);
        }

        protected virtual void Store(List<IAggregateRoot> updateRoots, List<IAggregateRoot> addRoots)
        {
            try
            {
                foreach (var item in addRoots)
                {
                    this._aggregateRootRepositoryContext.Add(item);
                }
                foreach (var item in updateRoots)
                {
                    this._aggregateRootRepositoryContext.Update(item);
                }
                this._aggregateRootRepositoryContext.Commit();
            }
            catch (Exception ex)
            {
                this._logger.LogError(LogEvent.EventHandle, ex, "事件回溯存储聚合根失败");
            }
        }


        private List<IAggregateRoot> HandleEventStream(IEnumerable<IAggregateRoot> cacheAggregateRoots, EventStream stream)
        {
            List<IAggregateRoot> addAggregateRoots = new List<IAggregateRoot>();
            foreach (var item in stream.Sources)
            {
                var type = this._domainTypeProvider.GetType(item.AggregateRootTypeName);
                var isAdd = item.Event.Version <= 1;
                var root = cacheAggregateRoots
                            .Where(a => a.Id == item.AggregateRootId && a.GetType() == type)
                            .FirstOrDefault();
                if (root == null && !isAdd)
                {
                    root = addAggregateRoots.Where(a => a.Id == item.AggregateRootId && a.GetType() == type)
                            .FirstOrDefault();
                }
                if (isAdd)
                {
                    root = Activator.CreateInstance(type, true) as IAggregateRoot;
                    addAggregateRoots.Add(root);
                }
                this._replayEventHandler.Handle(root, item.Event);
            }
            return addAggregateRoots;
        }


        public void Handle(DomainMessage<EventStream>[] messages)
        {
            var sources = messages.SelectMany(a => a.Content.Sources);
            var updateEvents = sources.Where(a => a.Event.Version > 1).GroupBy(a => a.AggregateRootTypeName);

            List<IAggregateRoot> updateRoots = new List<IAggregateRoot>();
            List<IAggregateRoot> addRoots = new List<IAggregateRoot>();
            List<ReplyMessage> replyMessages = new List<ReplyMessage>();
            //批量获取要更新的聚合根，并回溯事件生成新的状态保存
            try
            {
                foreach (var item in updateEvents)
                {
                    var keys = item.Select(a => a.AggregateRootId).Distinct().ToArray();
                    var type = this._domainTypeProvider.GetType(item.Key);

                    var roots = this._aggregateRootQuery.Get(type, keys);
                    updateRoots.AddRange(roots);
                }
            }
            catch (Exception ex)
            {
                foreach (var item in messages)
                    if (CheckNotify(item))
                        replyMessages.Add(this.BuildReplyMessage(item.ReplyKey, ex));

                this._logger.LogError(LogEvent.EventHandle, ex, "查询回滚对象");
                this.Notify(replyMessages);
                return;
            }

            foreach (var item in messages)
            {
                try
                {
                    var adds = this.HandleEventStream(updateRoots.Concat(addRoots), item.Content);
                    addRoots.AddRange(adds);
                    if (CheckNotify(item))
                        replyMessages.Add(this.BuildReplyMessage(item.ReplyKey));
                }
                catch (Exception ex)
                {
                    this._logger.LogError(LogEvent.EventHandle, ex, "事件回溯失败:" + string.Join(",", item.Content.Sources.Select(a => a.Id)));
                }
            }

            this.Store(updateRoots, addRoots);
            this.Notify(replyMessages);
        }
    }
}
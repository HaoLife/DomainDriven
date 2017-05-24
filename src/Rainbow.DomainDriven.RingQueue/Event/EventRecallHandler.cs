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
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.Cache;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class EventRecallHandler : IMessageHandler<DomainMessage<EventStream>>
    {
        private readonly IReplayEventProxy _replayEventProxy;
        private readonly IAggregateRootCommonQuery _aggregateRootCommonQuery;
        private readonly IDomainTypeProvider _domainTypeSearch;
        private readonly IReplyMessageListening _replyMessageListening;
        private readonly IAggregateRootCache _aggregateRootCache;
        private readonly IAggregateRootRepositoryContext _aggregateRootRepositoryContext;


        public EventRecallHandler(
            IReplayEventProxy replayEventProxy
            , IAggregateRootCommonQuery aggregateRootCommonQuery
            , IDomainTypeProvider domainTypeSearch
            , IReplyMessageListening replyMessageListening
            , IAggregateRootCache aggregateRootCache
            , IAggregateRootRepositoryContext aggregateRootRepositoryContext)
        {
            this._replayEventProxy = replayEventProxy;
            this._aggregateRootCommonQuery = aggregateRootCommonQuery;
            this._domainTypeSearch = domainTypeSearch;
            this._replyMessageListening = replyMessageListening;
            this._aggregateRootCache = aggregateRootCache;
            this._aggregateRootRepositoryContext = aggregateRootRepositoryContext;
        }

        protected virtual bool CheckNotify(MessageHead head)
        {
            return Consistency.Strong == head.Consistency && !string.IsNullOrEmpty(head.ReplyTo);
        }


        protected virtual ReplyMessage BuildReplyMessage(string replyKey, Exception ex = null)
        {
            var replyMessage = new ReplyMessage() { IsSuccess = ex != null, Exception = ex, ReplyKey = replyKey };
            return replyMessage;
        }


        protected virtual void Notify(List<ReplyMessage> replyMessages)
        {
            foreach (var item in replyMessages)
                this._replyMessageListening.Add(item);
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

            }
        }


        private List<IAggregateRoot> HandleEventStream(IEnumerable<IAggregateRoot> cacheAggregateRoots, EventStream stream)
        {
            List<IAggregateRoot> addAggregateRoots = new List<IAggregateRoot>();
            foreach (var item in stream.Sources)
            {
                var type = this._domainTypeSearch.GetType(item.AggregateRootTypeName);
                var isAdd = item.Event.Version <= 1;
                var root = cacheAggregateRoots
                            .Where(a => a.Id == item.AggregateRootId && a.GetType() == type)
                            .FirstOrDefault();
                if (isAdd)
                {
                    root = Activator.CreateInstance(type) as IAggregateRoot;
                    addAggregateRoots.Add(root);
                }
                this._replayEventProxy.Handle(root, item.Event);
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
            //todo:批量获取要更新的聚合根，并回溯事件生成新的状态保存
            foreach (var item in updateEvents)
            {
                var keys = item.Select(a => a.AggregateRootId).Distinct().ToArray();
                var type = this._domainTypeSearch.GetType(item.Key);
                var roots = this._aggregateRootCommonQuery.Get(type, keys);
                updateRoots.AddRange(roots);
            }
            foreach (var item in messages)
            {
                var adds = this.HandleEventStream(updateRoots.Concat(addRoots), item.Content);
                addRoots.AddRange(adds);
                if (CheckNotify(item.Head))
                    replyMessages.Add(this.BuildReplyMessage(item.Head.ReplyTo));
            }

            this.Store(updateRoots, addRoots);
            this.Notify(replyMessages);
        }
    }
}
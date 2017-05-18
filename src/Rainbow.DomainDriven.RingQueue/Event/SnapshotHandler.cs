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
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.DomainDriven.Cache;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class SnapshotHandler : IMessageHandler<DomainMessage>
    {
        private readonly IReplayEventProxyProvider _replayEventProxyProvider;
        private readonly IAggregateRootBatchRepositoryProvider _aggregateRootBatchRepositoryProvider;
        private readonly IDomainTypeProvider _domainTypeSearch;
        private readonly ILogger<SnapshotHandler> _logger;
        private readonly IMessageListening _messageListening;
        private readonly IAggregateRootCache _aggregateRootCache;


        public SnapshotHandler(
            IReplayEventProxyProvider replayEventProxyProvider
            , IAggregateRootBatchRepositoryProvider aggregateRootBatchRepositoryProvider
            , IDomainTypeProvider domainTypeSearch
            , IMessageListening messageListening
            , IAggregateRootCache aggregateRootCache
            , ILogger<SnapshotHandler> logger
            )
        {
            this._replayEventProxyProvider = replayEventProxyProvider;
            this._aggregateRootBatchRepositoryProvider = aggregateRootBatchRepositoryProvider;
            this._domainTypeSearch = domainTypeSearch;
            this._messageListening = messageListening;
            this._aggregateRootCache = aggregateRootCache;
            this._logger = logger;
        }
        public void HandleEventStream(List<IAggregateRoot> usedAggregates, ConcurrentDictionary<Type, IAggregateRootBatchRepository> unRepo, EventStream stream)
        {
            foreach (var item in stream.EventSources)
            {
                var type = this._domainTypeSearch.GetType(item.AggregateRootTypeName);
                IAggregateRoot root;
                var repo = unRepo.GetOrAdd(type, t => _aggregateRootBatchRepositoryProvider.GetRepo(t));
                if (item.Event.Version <= 1)
                {
                    root = Activator.CreateInstance(type) as IAggregateRoot;
                    var proxy = _replayEventProxyProvider.GetReplayEventProxy(item.Event.GetType());
                    proxy.Handle(root, item.Event);
                    repo.Add(root);
                }
                else
                {
                    root = repo.Get(item.AggregateRootId);
                    var proxy = _replayEventProxyProvider.GetReplayEventProxy(item.Event.GetType());
                    proxy.Handle(root, item.Event);
                    repo.Update(root);
                }
                usedAggregates.Add(root);
            }
        }

        public void Commit(List<IAggregateRoot> usedAggregates, ConcurrentDictionary<Type, IAggregateRootBatchRepository> unRepo)
        {
            foreach (var item in unRepo.Values)
            {
                try
                {
                    item.Commit();
                }
                catch (Exception ex)
                {
                    this._logger.LogError(LogEvent.Frame, ex, $"execute command:{nameof(SnapshotHandler)} error by commit ");
                }
            }
            foreach (var item in usedAggregates)
                this._aggregateRootCache.RemoveWhere(item);
        }


        private void Notice(List<DomainMessage> data, bool isSuccess, Exception ex = null)
        {
            var message = new NoticeMessage() { IsSuccess = isSuccess, Exception = ex };
            foreach (var item in data)
            {
                if (item.Head.Consistency == Consistency.Finally && !string.IsNullOrEmpty(item.Head.ReplyKey))
                    this._messageListening.Notice(item.Head.ReplyKey, message);
            }
        }
        private void Notice(DomainMessage domainMessage, bool isSuccess, Exception ex = null)
        {
            if (domainMessage.Head.Consistency == Consistency.Finally && !string.IsNullOrEmpty(domainMessage.Head.ReplyKey))
            {
                var message = new NoticeMessage() { IsSuccess = isSuccess, Exception = ex };
                this._messageListening.Notice(domainMessage.Head.ReplyKey, message);
            }
        }

        public void Handle(DomainMessage[] messages)
        {
            var data = messages.ToList();
            ConcurrentDictionary<Type, IAggregateRootBatchRepository> unRepo = new ConcurrentDictionary<Type, IAggregateRootBatchRepository>();
            List<IAggregateRoot> usedAggregates = new List<IAggregateRoot>();
            foreach (var message in messages)
            {
                try
                {
                    var stream = message.Content as EventStream;
                    if (stream == null) return;

                    this.HandleEventStream(usedAggregates, unRepo, stream);
                }
                catch (Exception ex)
                {
                    data.Remove(message);
                    Notice(message, false, ex);
                    this._logger.LogError(LogEvent.Frame, ex, $"execute name:{nameof(SnapshotHandler)} error by message:");
                }

                try
                {
                    this.Commit(usedAggregates, unRepo);
                    Notice(data, true);
                    unRepo.Clear();
                    usedAggregates.Clear();
                }
                catch (Exception ex)
                {
                    this._logger.LogError(LogEvent.Frame, ex, $"execute name:{nameof(SnapshotHandler)} error");
                    Notice(data, false, ex);
                }
            }
        }
    }
}
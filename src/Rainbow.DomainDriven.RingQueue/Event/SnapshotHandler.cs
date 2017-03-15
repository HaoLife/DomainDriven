using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.RingQueue.Queue;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Infrastructure;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.DomainDriven.Cache;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class SnapshotHandler : IQueueHandler<DomainMessage>
    {
        private readonly IReplayEventProxyProvider _replayEventProxyProvider;
        private readonly IAggregateRootBatchRepositoryProvider _aggregateRootBatchRepositoryProvider;
        private readonly IDomainTypeProvider _domainTypeSearch;
        private readonly ILogger<SnapshotHandler> _logger;
        private readonly IMessageListening _messageListening;
        private readonly IAggregateRootCache _aggregateRootCache;

        private ConcurrentDictionary<Type, IAggregateRootBatchRepository> _unRepo;
        private List<DomainMessage> _data;
        private List<IAggregateRoot> _usedAggregates;

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
            this._unRepo = new ConcurrentDictionary<Type, IAggregateRootBatchRepository>();
            this._data = new List<DomainMessage>();
            this._usedAggregates = new List<IAggregateRoot>();
        }
        public void Handle(DomainMessage message, long sequence, bool isEnd)
        {
            try
            {
                var stream = message.Content as DomainEventStream;
                if (stream == null) return;

                this._data.Add(message);
                this.HandleEventStream(stream);
            }
            catch (Exception ex)
            {
                this._data.Remove(message);
                Notice(message, false, ex);
                this._logger.LogError(LogEvent.Frame, ex, $"execute name:{nameof(SnapshotHandler)} error by message:");
            }

            if (!isEnd) return;

            try
            {
                this.Commit();
                Notice(true);
                this._unRepo.Clear();
            }
            catch (Exception ex)
            {
                this._logger.LogError(LogEvent.Frame, ex, $"execute name:{nameof(SnapshotHandler)} error");
                Notice(false, ex);
            }

        }
        public void HandleEventStream(DomainEventStream stream)
        {
            foreach (var item in stream.EventSources)
            {
                var type = this._domainTypeSearch.GetType(item.AggregateRootTypeName);
                IAggregateRoot root;
                var repo = _unRepo.GetOrAdd(type, t => _aggregateRootBatchRepositoryProvider.GetRepo(t));
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
                this._usedAggregates.Add(root);
            }
        }

        public void Commit()
        {
            foreach (var item in _unRepo.Values)
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
            foreach (var item in this._usedAggregates)
                this._aggregateRootCache.RemoveWhere(item);
            this._usedAggregates.Clear();
        }


        private void Notice(bool isSuccess, Exception ex = null)
        {
            var message = new NoticeMessage() { IsSuccess = isSuccess, Exception = ex };
            foreach (var item in this._data)
            {
                if (item.Head.Consistency == ConsistencyLevel.Finally && !string.IsNullOrEmpty(item.Head.ReplyKey))
                    this._messageListening.Notice(item.Head.ReplyKey, message);
            }
        }
        private void Notice(DomainMessage domainMessage, bool isSuccess, Exception ex = null)
        {
            if (domainMessage.Head.Consistency == ConsistencyLevel.Finally && !string.IsNullOrEmpty(domainMessage.Head.ReplyKey))
            {
                var message = new NoticeMessage() { IsSuccess = isSuccess, Exception = ex };
                this._messageListening.Notice(domainMessage.Head.ReplyKey, message);
            }
        }
    }
}
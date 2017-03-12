using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.RingQueue.Queue;
using System.Linq;
using Rainbow.DomainDriven.Cache;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class CommandCacheHandler : IQueueHandler<DomainMessage>
    {
        private readonly CommandMappingOptions _commandMappingOptions;

        private readonly IAggregateRootCommonQueryRepository _aggregateRootCommonQueryRepository;
        private readonly IAggregateRootCache _aggregateRootCache;
        private readonly ILogger<CommandCacheHandler> _logger;
        private ConcurrentDictionary<Type, List<Guid>> _data;
        public CommandCacheHandler(
            ICommandMappingProvider commandMappingProvider,
            IAggregateRootCommonQueryRepository aggregateRootCommonQueryRepository,
            IAggregateRootCache aggregateRootCache,
            ILogger<CommandCacheHandler> logger
        )
        {
            this._aggregateRootCommonQueryRepository = aggregateRootCommonQueryRepository;
            this._aggregateRootCache = aggregateRootCache;
            this._logger = logger;
            this._commandMappingOptions = new CommandMappingOptions();
            commandMappingProvider.OnConfiguring(this._commandMappingOptions);
            this._data = new ConcurrentDictionary<Type, List<Guid>>();
        }
        public void Handle(DomainMessage message, long sequence, bool isEnd)
        {
            var mapValue = this._commandMappingOptions.FindMap(message.Content);
            foreach (var item in mapValue)
                this._data.AddOrUpdate(item.Value, AddValue(new List<Guid>(), item.Key), (a, b) => AddValue(b, item.Key));
            if (!isEnd) return;

            foreach (var item in this._data)
            {
                var caches = item.Value.Where(a => _aggregateRootCache.Exists(item.Key, a)).ToArray();
                var count = this._aggregateRootCache.Use(item.Key, caches);
                if (count != caches.Length)
                {
                    caches = item.Value.Where(a => _aggregateRootCache.Exists(item.Key, a)).ToArray();
                }
                var reads = item.Value.Where(a => !_aggregateRootCache.Exists(item.Key, a)).ToArray();
                var aggregateRoots = this._aggregateRootCommonQueryRepository.Get(item.Key, reads);
                foreach (var aggr in aggregateRoots)
                    this._aggregateRootCache.Set(aggr);
                this._aggregateRootCache.Use(aggregateRoots);

                var keys = aggregateRoots.Select(a => a.Id).ToArray();
                var invalids = reads.Where(a => !keys.Contains(a)).ToList();
                foreach (var invalid in invalids)
                    this._aggregateRootCache.SetInvalid(item.Key, invalid);
            }

        }

        public List<Guid> AddValue(List<Guid> source, Guid key)
        {
            source.Add(key);
            return source;
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using System.Linq;
using Rainbow.DomainDriven.Cache;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class CommandCacheHandler : IMessageHandler<DomainMessage<ICommand>>
    {
        private readonly CommandMapper _commandMapper;
        private readonly IAggregateRootCommonQuery _aggregateRootCommonQueryRepository;
        private readonly IAggregateRootCache _aggregateRootCache;
        private readonly ILogger<CommandCacheHandler> _logger;

        public CommandCacheHandler(
            ICommandMappingProvider commandMappingProvider,
            IAggregateRootCommonQuery aggregateRootCommonQueryRepository,
            IAggregateRootCache aggregateRootCache,
            ILogger<CommandCacheHandler> logger
        )
        {
            this._aggregateRootCommonQueryRepository = aggregateRootCommonQueryRepository;
            this._aggregateRootCache = aggregateRootCache;
            this._logger = logger;
            this._commandMapper = new CommandMapper();
            commandMappingProvider.OnConfiguring(this._commandMapper);
        }


        public List<Guid> AddValue(List<Guid> source, Guid key)
        {
            source.Add(key);
            return source;
        }

        public void Handle(DomainMessage<ICommand>[] messages)
        {
            ConcurrentDictionary<Type, List<Guid>> data = new ConcurrentDictionary<Type, List<Guid>>();

            foreach (var message in messages)
            {
                var mapValue = this._commandMapper.FindMap(message.Content);
                foreach (var item in mapValue)
                    data.AddOrUpdate(item.Value, AddValue(new List<Guid>(), item.Key), (a, b) => AddValue(b, item.Key));
            }


            foreach (var item in data)
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

    }
}
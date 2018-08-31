using Rainbow.DomainDriven.Command;
using Rainbow.MessageQueue.Ring;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    class RingCommandCacheHandler : AbstractBatchMessageHandler<ICommand>
    {
        private readonly ICommandMappingProvider _commandMappingProvider;
        private readonly IAggregateRootRebuilder _aggregateRootRebuilder;
        private readonly IContextCache _contextCache;
        private readonly ILogger _logger;

        public RingCommandCacheHandler(
            ICommandMappingProvider commandMappingProvider,
            IAggregateRootRebuilder aggregateRootRebuilder,
            IContextCache contextCache,
            ILoggerFactory loggerFactory
        )
        {
            this._aggregateRootRebuilder = aggregateRootRebuilder;
            this._contextCache = contextCache;
            this._logger = loggerFactory.CreateLogger<RingCommandCacheHandler>();
            this._commandMappingProvider = commandMappingProvider;
        }


        public List<Guid> AddValue(List<Guid> source, Guid key)
        {
            source.Add(key);
            return source;
        }

        public override void Handle(ICommand[] messages, long endSequence)
        {
            ConcurrentDictionary<Type, List<Guid>> data = new ConcurrentDictionary<Type, List<Guid>>();

            foreach (var message in messages)
            {
                var mapValue = this._commandMappingProvider.Find(message);
                foreach (var item in mapValue)
                    data.AddOrUpdate(item.Value, AddValue(new List<Guid>(), item.Key), (a, b) => AddValue(b, item.Key));
            }


            foreach (var item in data)
            {
                var caches = item.Value.Where(a => _contextCache.Exists(item.Key, a)).ToArray();

                var reads = item.Value.Where(a => !_contextCache.Exists(item.Key, a)).ToArray();
                var aggregateRoots = this._aggregateRootRebuilder.Rebuild(item.Key, reads);
                foreach (var aggr in aggregateRoots)
                    this._contextCache.Set(aggr);

                var keys = aggregateRoots.Select(a => a.Id).ToArray();
                var invalids = reads.Where(a => !keys.Contains(a)).ToList();
                foreach (var invalid in invalids)
                    this._contextCache.Set(item.Key, invalid, null);
            }
        }

    }
}

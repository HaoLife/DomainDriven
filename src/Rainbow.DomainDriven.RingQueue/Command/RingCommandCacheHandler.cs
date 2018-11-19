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
    class RingCommandCacheHandler : AbstractBatchMessageHandler<CommandMessage>
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
        ) : base(1000)
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

        private void HandleMessageMapping(ConcurrentDictionary<Type, List<Guid>> data, ICommand cmd)
        {
            try
            {
                var mapValue = this._commandMappingProvider.Find(cmd);
                foreach (var item in mapValue)
                    data.AddOrUpdate(item.Value, AddValue(new List<Guid>(), item.Key), (a, b) => AddValue(b, item.Key));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"执行缓存映射错误，无法正确进行缓存，类型：{cmd?.GetType().Name}");

            }
        }

        private void HandleCache(Type rootType, IEnumerable<Guid> keys)
        {
            try
            {

                //获取本地缓存存在的数据
                var caches = keys.Where(a => _contextCache.Exists(rootType, a)).ToArray();

                //获取需要读取的数据
                var reads = keys.Where(a => !caches.Contains(a)).ToArray();
                //重建聚合根（这里需要使用重建，而不能使用快照，快照可能存在数据未更新即使的问题）
                var aggregateRoots = this._aggregateRootRebuilder.Rebuild(rootType, reads);
                foreach (var aggr in aggregateRoots)
                    this._contextCache.Set(aggr);
                //获取无法找到的key
                var rebuilderKeys = aggregateRoots.Select(a => a.Id).ToArray();
                var invalids = reads.Where(a => !rebuilderKeys.Contains(a)).ToList();
                foreach (var invalid in invalids)
                    this._contextCache.Set(rootType, invalid, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取缓存失败：{rootType.Name} - keys:{string.Join(",", keys)}");
            }


        }

        public override void Handle(CommandMessage[] messages, long endSequence)
        {
            ConcurrentDictionary<Type, List<Guid>> data = new ConcurrentDictionary<Type, List<Guid>>();

            foreach (var message in messages)
            {
                HandleMessageMapping(data, message.Cmd);
            }

            foreach (var item in data)
            {
                HandleCache(item.Key, item.Value);
            }
        }

    }
}

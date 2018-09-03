using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Store;
using Rainbow.MessageQueue.Ring;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq.Expressions;
using Rainbow.DomainDriven.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class RingEventSnapshootHandler : AbstractBatchMessageHandler<IEvent>
    {
        private static readonly MethodInfo _handleMethod = typeof(RingEventSnapshootHandler).GetMethod(nameof(GetSnapshotStore), BindingFlags.Instance | BindingFlags.NonPublic);
        private ConcurrentDictionary<Type, Func<ISnapshootStore>> _cache = new ConcurrentDictionary<Type, Func<ISnapshootStore>>();
        private ConcurrentDictionary<string, Type> _cacheEntitys = new ConcurrentDictionary<string, Type>();

        private IAssemblyProvider _assemblyProvider;
        private ISnapshootStoreFactory _snapshootStoreFactory;
        private IEventRebuildHandler _eventRebuildHandler;
        private ILogger<RingEventSnapshootHandler> _logger;
        private ISubscribeEventStore _subscribeEventStore;
        private IMemoryCache _memoryCache;


        private static Guid _defaultSubscribeId = new Guid("00000000-0000-0000-0000-000000000001");
        private SubscribeEvent _subscribeEvent = new SubscribeEvent() { Id = _defaultSubscribeId, UTCTimestamp = 0 };


        public RingEventSnapshootHandler(
            IAssemblyProvider assemblyProvider
            , ISnapshootStoreFactory snapshootStoreFactory
            , IEventRebuildHandler eventRebuildHandler
            , ISubscribeEventStore subscribeEventStore
            , IMemoryCache memoryCache
            , ILoggerFactory loggerFactory)
        {
            this._assemblyProvider = assemblyProvider;
            _snapshootStoreFactory = snapshootStoreFactory;
            _eventRebuildHandler = eventRebuildHandler;
            _subscribeEventStore = subscribeEventStore;
            _memoryCache = memoryCache;
            _logger = loggerFactory.CreateLogger<RingEventSnapshootHandler>();


            this.Initialize(assemblyProvider.Assemblys);

            var subscribeEvent = _subscribeEventStore.Get(_defaultSubscribeId);
            if (subscribeEvent != null) _subscribeEvent = subscribeEvent;
        }


        private void Initialize(IEnumerable<Assembly> assemblys)
        {
            this.RegisterHandler(assemblys.SelectMany(p => p.GetTypes()));
        }



        private void RegisterHandler(Type type)
        {
            if (!typeof(IAggregateRoot).GetTypeInfo().IsAssignableFrom(type) && type.IsClass) return;

            _cacheEntitys.TryAdd(type.Name, type);

        }

        private void RegisterHandler(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                this.RegisterHandler(type);
            }
        }


        public override void Handle(IEvent[] messages, long endSequence)
        {

            messages = messages.Where(a => a.UTCTimestamp > _subscribeEvent.UTCTimestamp).ToArray();
            if (!messages.Any()) return;

            var dict = messages.GroupBy(a => a.AggregateRootTypeName).ToDictionary(a => a.Key, a => a.ToList());

            List<IAggregateRoot> aggregateRoots = new List<IAggregateRoot>();
            List<IAggregateRoot> removeRoots = new List<IAggregateRoot>();
            List<IAggregateRoot> addRoots = new List<IAggregateRoot>();

            //获取
            foreach (var item in dict)
            {
                Type entityType;
                if (!_cacheEntitys.TryGetValue(item.Key, out entityType))
                {
                    throw new Exception($"无法找到类型{item.Key}的聚合根");
                }

                //这里可以先从缓存获取，如果没有全部找到，再从数据库拿没有取到的。

                var createIds = item.Value.Where(a => a.Operation == EventOperation.Created).Select(a => a.AggregateRootId).ToArray();

                var ids = item.Value.Select(a => a.AggregateRootId).Except(createIds).ToArray();


                List<IAggregateRoot> roots = new List<IAggregateRoot>();
                foreach (var id in ids)
                {
                    var root = _memoryCache.Get<IAggregateRoot>($"{entityType.Name}_{id}");
                    if (root != null) roots.Add(root);
                }
                var storeIds = ids.Except(roots.Select(a => a.Id)).ToArray();
                if (storeIds.Length > 0)
                {
                    var storeRoots = GetSnapshot(entityType).Get(storeIds);
                    roots.AddRange(storeRoots);
                }

                aggregateRoots.AddRange(roots);
            }

            //重建
            foreach (var item in messages)
            {
                IAggregateRoot root = aggregateRoots.FirstOrDefault(a => a.Id == item.AggregateRootId);

                if (item.Operation == EventOperation.Created)
                {
                    Type entityType;
                    if (!_cacheEntitys.TryGetValue(item.AggregateRootTypeName, out entityType))
                    {
                        throw new Exception($"无法找到类型{item.AggregateRootTypeName}的聚合根");
                    }

                    root = Activator.CreateInstance(entityType, true) as IAggregateRoot;
                    aggregateRoots.Add(root);
                    addRoots.Add(root);

                }
                else if (item.Operation == EventOperation.Removed)
                {
                    removeRoots.Add(root);
                }
                if (root == null)
                {
                    _logger.LogInformation($"回溯快照的时候没有找到聚合根：[{item.AggregateRootId} - {item.AggregateRootTypeName}]");
                    continue;
                }
                _eventRebuildHandler.Handle(root, item);

            }


            var rootDict = aggregateRoots.GroupBy(a => a.GetType()).ToDictionary(a => a.Key, a => a.ToList());

            foreach (var item in rootDict)
            {
                var snapshootStore = GetSnapshot(item.Key);

                var addeds = item.Value.Intersect(addRoots).ToArray();
                var removed = item.Value.Intersect(removeRoots).ToArray();
                var updated = item.Value.Except(addeds).Except(removed).ToArray();

                //存储快照
                TryAction(list => snapshootStore.Add(list), addeds);
                TryAction(list => snapshootStore.Remove(list), removed);
                TryAction(list => snapshootStore.Update(list), updated);


            }

            //存储缓存
            aggregateRoots.Except(removeRoots).ToList().ForEach(a =>
            {
                _memoryCache.Set($"{a.GetType().Name}_{a.Id}", a);
            });

            removeRoots.ForEach(a =>
            {
                _memoryCache.Remove($"{a.GetType().Name}_{a.Id}");
            });


            //记录快照消费者处理的的最后的事件

            var evt = messages.LastOrDefault();
            if (evt != null)
            {
                _subscribeEvent.AggregateRootId = evt.AggregateRootId;
                _subscribeEvent.AggregateRootTypeName = evt.AggregateRootTypeName;
                _subscribeEvent.EventId = evt.Id;
                _subscribeEvent.UTCTimestamp = evt.UTCTimestamp;
                _subscribeEventStore.Save(_subscribeEvent);
            }
        }

        private void TryAction(Action<IAggregateRoot[]> call, IAggregateRoot[] roots, int retry = 1)
        {
            try
            {
                if (roots.Length > 0)
                    call(roots);
            }
            catch (Exception ex)
            {
                _logger.LogError($"执行存储快照失败,重试次数{retry} - {ex.Message}", ex);
                if (retry >= 3)
                    TryAction(call, roots, retry + 1);
            }
        }


        private ISnapshootStore GetSnapshot(Type rootType)
        {

            var call = _cache.GetOrAdd(
                key: rootType,
                valueFactory: (type) =>
                {
                    var getHandleMethod = _handleMethod.MakeGenericMethod(type);
                    var instance = Expression.Constant(this);
                    var expression =
                        Expression.Lambda<Func<ISnapshootStore>>(
                            Expression.Call(instance, getHandleMethod));
                    return expression.Compile();
                });

            return call();
        }


        private ISnapshootStore GetSnapshotStore<TAggregateRoot>() where TAggregateRoot : class, IAggregateRoot
        {
            return _snapshootStoreFactory.Create<TAggregateRoot>();
        }
    }
}

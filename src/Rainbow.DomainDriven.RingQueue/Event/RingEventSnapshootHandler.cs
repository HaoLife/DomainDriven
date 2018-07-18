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

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class RingEventSnapshootHandler : IMessageHandler<IEvent>
    {
        private static readonly MethodInfo _handleCommandMethod = typeof(RingEventSnapshootHandler).GetMethod(nameof(GetSnapshot), BindingFlags.Instance | BindingFlags.NonPublic);
        private ConcurrentDictionary<Type, Func<ISnapshootStore>> _cache = new ConcurrentDictionary<Type, Func<ISnapshootStore>>();
        private ConcurrentDictionary<string, Type> _cacheEntitys = new ConcurrentDictionary<string, Type>();

        private IAssemblyProvider _assemblyProvider;
        private ISnapshootStoreFactory _snapshootStoreFactory;
        private IEventRebuildHandler _eventRebuildHandler;
        private ILogger<RingEventSnapshootHandler> _logger;

        public RingEventSnapshootHandler(IAssemblyProvider assemblyProvider)
        {
            this.Initialize(assemblyProvider.Assemblys);
        }


        private void Initialize(IEnumerable<Assembly> assemblys)
        {
            this.RegisterHandler(assemblys.SelectMany(p => p.GetTypes()));
        }



        private void RegisterHandler(Type type)
        {
            if (!typeof(IAggregateRoot).GetTypeInfo().IsAssignableFrom(type)) return;

            _cacheEntitys.TryAdd(type.Name, type);

        }

        private void RegisterHandler(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                this.RegisterHandler(type);
            }
        }


        public void Handle(IEvent[] messages)
        {
            var dict = messages.GroupBy(a => a.AggregateRootTypeName).ToDictionary(a => a.Key, a => a.ToList());

            List<IAggregateRoot> aggregateRoots = new List<IAggregateRoot>();
            List<IAggregateRoot> removeRoots = new List<IAggregateRoot>();

            foreach (var item in dict)
            {
                Type entityType;
                if (!_cacheEntitys.TryGetValue(item.Key, out entityType))
                {
                    throw new Exception($"无法找到类型{item.Key}的聚合根");
                }
                var roots = GetSnapshot(entityType).Get(item.Value.Select(a => a.AggregateRootId).ToArray());

                aggregateRoots.AddRange(roots);
            }

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

                }
                else if (item.Operation == EventOperation.Removed)
                {
                    removeRoots.Add(root);
                }
                _eventRebuildHandler.Handle(root, item);

            }


            var rootDict = aggregateRoots.GroupBy(a => a.GetType()).ToDictionary(a => a.Key, a => a.ToList());

            foreach (var item in rootDict)
            {
                var snapshootStore = GetSnapshot(item.Key);
                var addeds = item.Value.Where(a => a.Version == 1).ToArray();
                var removed = item.Value.Intersect(removeRoots).ToArray();
                var updated = item.Value.Except(addeds).Except(removed).ToArray();

                //存储快照
                TryAction(list => snapshootStore.Add(list), addeds);
                TryAction(list => snapshootStore.Add(list), removed);
                TryAction(list => snapshootStore.Add(list), updated);

            }


            //记录快照消费者处理的的最后的事件
        }

        private void TryAction(Action<IAggregateRoot[]> call, IAggregateRoot[] roots, int retry = 0)
        {
            try
            {
                if (roots.Length > 0)
                    call(roots);
            }
            catch (Exception ex)
            {
                _logger.LogError($"执行存储快照失败,重试次数{retry}", ex);
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
                    var getHandleMethod = _handleCommandMethod.MakeGenericMethod(type);
                    var expression =
                        Expression.Lambda<Func<ISnapshootStore>>(
                            Expression.Call(null, getHandleMethod));
                    return expression.Compile();
                });

            return call();
        }


        public ISnapshootStore GetSnapshotStore<TAggregateRoot>() where TAggregateRoot : class, IAggregateRoot
        {
            return _snapshootStoreFactory.Create<TAggregateRoot>();
        }
    }
}

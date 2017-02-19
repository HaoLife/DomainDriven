using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Rainbow.DomainDriven.Event
{
    public class EventHandlerProxyProvider : IEventHandlerProxyProvider
    {

        private readonly IServiceProvider _serviceProvider;

        // private readonly ConcurrentDictionary<Type, IEventHandlerProxy> _cacheAggregateRootRepo =
        //        new ConcurrentDictionary<Type, IEventHandlerProxy>();

        public EventHandlerProxyProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        // public void Initialize(IEnumerable<Assembly> assemblys)
        // {
        //     this.RegisterExecutor(assemblys.SelectMany(p => p.GetTypes()));
        // }

        // private void RegisterExecutor(Type type)
        // {
        //     var info = type.GetTypeInfo();
        //     if (!(info.IsClass && typeof(IEvent).IsAssignableFrom(type))) return;

        //     var genericType = typeof(EventHandlerProxy<>).MakeGenericType(type);
        //     var createFactory = ActivatorUtilities.CreateFactory(genericType, Type.EmptyTypes);
        //     _cacheAggregateRootRepo.TryAdd(type, (IEventHandlerProxy)createFactory(_serviceProvider, arguments: null));
        // }

        // private void RegisterExecutor(IEnumerable<Type> types)
        // {
        //     foreach (var item in types)
        //     {
        //         this.RegisterExecutor(item);
        //     }
        // }


        public IEventHandlerProxy GetEventHanderProxy(Type eventType)
        {
            // IEventHandlerProxy repo;
            // if (_cacheAggregateRootRepo.TryGetValue(eventType, out repo)) return repo;
            var genericType = typeof(EventHandlerProxy<>).MakeGenericType(eventType);
            var proxy = this._serviceProvider.GetService(genericType);
            if (proxy != null) return proxy as IEventHandlerProxy;

            throw new NotImplementedException($"没有找到类型：{eventType.Name}");
        }
    }
}
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

        public EventHandlerProxyProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public IEventHandlerProxy GetEventHanderProxy(Type eventType)
        {
            var genericType = typeof(EventHandlerProxy<>).MakeGenericType(eventType);
            var proxy = this._serviceProvider.GetService(genericType);
            if (proxy != null) return proxy as IEventHandlerProxy;

            throw new NotImplementedException($"没有找到类型：{eventType.Name}");
        }
    }
}
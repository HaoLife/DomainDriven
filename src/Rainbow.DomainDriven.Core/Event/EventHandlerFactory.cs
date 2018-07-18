using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Event
{
    public class EventHandlerFactory : IEventHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Func<Type, IServiceProvider, object> _createInstance =
            (type, provider) => ActivatorUtilities.CreateInstance(provider, type, Type.EmptyTypes);

        private readonly ConcurrentDictionary<Type, object> _typeActivatorCache =
               new ConcurrentDictionary<Type, object>();

        public EventHandlerFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public IEventHandler<TEvent> Create<TEvent>(Type type) where TEvent : IEvent
        {
            return (IEventHandler<TEvent>)_typeActivatorCache.GetOrAdd(type, t => _createInstance(t, _serviceProvider));
        }
    }
}

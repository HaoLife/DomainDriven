using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Event
{
    public class EventHandlerActivator : IEventHandlerActivator
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Func<Type, ObjectFactory> _createFactory =
            (type) => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes);

        private readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public EventHandlerActivator(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public IEventHandler<TEvent> Create<TEvent>(Type type) where TEvent : IEvent
        {
            var createFactory = _typeActivatorCache.GetOrAdd(type, _createFactory);
            return (IEventHandler<TEvent>)createFactory(_serviceProvider, arguments: null);
        }
    }
}

using Microsoft.Extensions.DependencyModel;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Infrastructure;

namespace Rainbow.DomainDriven.Event
{
    public class EventHandlerSelector : IEventHandlerSelector
    {
        private ConcurrentDictionary<Type, List<Type>> _cacheEventHandler = new ConcurrentDictionary<Type, List<Type>>();
    
        private readonly IAssemblyProvider _assemblyProvider;

        public EventHandlerSelector(IAssemblyProvider assemblyProvider)
        {
            this._assemblyProvider = assemblyProvider;
            this.Initialize(this._assemblyProvider.Assemblys);
        }

        public IEnumerable<Type> FindHandlerTypes<TEvent>() where TEvent : IEvent
        {
            List<Type> handleTypes;
            this._cacheEventHandler.TryGetValue(typeof(IEventHandler<TEvent>), out handleTypes);
            return handleTypes ?? new List<Type>();
        }


        private void Initialize(IEnumerable<Assembly> assemblys)
        {
            this.RegisterHandler(assemblys.SelectMany(p => p.GetTypes()));
        }


        private void RegisterHandler(Type handlerType)
        {
            if (typeof(IEntity).GetTypeInfo().IsAssignableFrom(handlerType)) return;

            var handlerInterfaceTypes = TypeHelper.GetGenericInterfaceTypes(handlerType, typeof(IEventHandler<>));

            foreach (var handlerInterface in handlerInterfaceTypes)
            {
                var types = this._cacheEventHandler.GetOrAdd(handlerInterface, new List<Type>());
                types.Add(handlerType);
            }
        }



        private void RegisterHandler(IEnumerable<Type> handlerTypes)
        {
            foreach (var handlerType in handlerTypes)
            {
                RegisterHandler(handlerType);
            }

        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Rainbow.DomainDriven.Utilities;
using Rainbow.DomainDriven.Domain;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Framework;

namespace Rainbow.DomainDriven.Event
{
    public class AutoEventRegister : IEventRegister
    {
        private IAssemblyProvider _assemblyProvider;
        private ConcurrentDictionary<Type, List<Type>> _cacheEventHandler = new ConcurrentDictionary<Type, List<Type>>();


        public AutoEventRegister(IAssemblyProvider assemblyProvider)
        {
            _assemblyProvider = assemblyProvider;
            this.Initialize(this._assemblyProvider.Assemblys);
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
                var types = this._cacheEventHandler.GetOrAdd(handlerInterface.GetGenericArguments().FirstOrDefault(), new List<Type>());
                types.Add(handlerType);
            }
        }

        private void RegisterHandler(IEnumerable<Type> handlerTypes)
        {
            foreach (var handlerType in handlerTypes)
            {
                this.RegisterHandler(handlerType);
            }
        }

        public void Add(Type eventType, Type handlerType)
        {
            if (this._cacheEventHandler.ContainsKey(eventType))
            {
                var events = this._cacheEventHandler.GetOrAdd(eventType, new List<Type>());
                events.Add(handlerType);
            }
        }

        public IEnumerable<Type> FindHandlerType<TEvent>() where TEvent : IEvent
        {
            List<Type> handleTypes;
            this._cacheEventHandler.TryGetValue(typeof(TEvent), out handleTypes);
            return handleTypes ?? new List<Type>();
        }
    }
}

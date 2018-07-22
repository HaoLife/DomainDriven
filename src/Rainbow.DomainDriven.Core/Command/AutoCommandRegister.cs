using Rainbow.DomainDriven.Infrastructure;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Utilities;

namespace Rainbow.DomainDriven.Command
{
    public class AutoCommandRegister : ICommandRegister
    {
        private IAssemblyProvider _assemblyProvider;
        private ConcurrentDictionary<Type, Type> _cacheCommandHandler = new ConcurrentDictionary<Type, Type>();

        public AutoCommandRegister(IAssemblyProvider assemblyProvider)
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
            var HandlerTypes = TypeHelper.GetGenericInterfaceTypes(handlerType, typeof(ICommandHandler<>));

            foreach (var execType in HandlerTypes)
            {
                var commandType = execType.GetGenericArguments().FirstOrDefault();
                if (!this._cacheCommandHandler.ContainsKey(commandType))
                    this._cacheCommandHandler.TryAdd(commandType, handlerType);
            }
        }

        private void RegisterHandler(IEnumerable<Type> handlerTypes)
        {
            foreach (var handlerType in handlerTypes)
            {
                this.RegisterHandler(handlerType);
            }
        }

        public void Add(Type commandType, Type handlerType)
        {
            if (this._cacheCommandHandler.ContainsKey(commandType))
                this._cacheCommandHandler.TryAdd(commandType, handlerType);
        }

        public Type FindHandlerType<TCommand>() where TCommand : ICommand
        {
            var handlerType = default(Type);
            this._cacheCommandHandler.TryGetValue(typeof(TCommand), out handlerType);
            return handlerType;
        }
    }
}

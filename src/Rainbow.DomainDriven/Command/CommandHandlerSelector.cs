using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Utilities;
using System.Linq;

namespace Rainbow.DomainDriven.Command
{
    public class CommandHandlerSelector : ICommandHandlerSelector
    {
        private ConcurrentDictionary<Type, Type> _cacheCommandHandler = new ConcurrentDictionary<Type, Type>();
        private readonly IAssemblyProvider _assemblyProvider;


        public CommandHandlerSelector(IAssemblyProvider assemblyProvider)
        {
            this._assemblyProvider = assemblyProvider;
            this.Initialize(this._assemblyProvider.Assemblys);
        }
        private void Initialize(IEnumerable<Assembly> assemblys)
        {
            this.RegisterHandler(assemblys.SelectMany(p => p.GetTypes()));
        }

        public virtual Type FindHandlerType<TCommand>() where TCommand : ICommand
        {
            var HandlerType = default(Type);
            this._cacheCommandHandler.TryGetValue(typeof(TCommand), out HandlerType);
            return HandlerType;
        }



        private void RegisterHandler(Type executorType)
        {
            var HandlerTypes = TypeHelper.GetGenericInterfaceTypes(executorType, typeof(ICommandHandler<>));

            foreach (var execType in HandlerTypes)
            {
                this._cacheCommandHandler.TryAdd(execType.GetGenericArguments().FirstOrDefault(), executorType);
            }
        }

        private void RegisterHandler(IEnumerable<Type> HandlerTypes)
        {
            foreach (var executorType in HandlerTypes)
            {
                this.RegisterHandler(executorType);
            }
        }
    }
}
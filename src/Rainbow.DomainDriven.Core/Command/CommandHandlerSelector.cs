using Rainbow.DomainDriven.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Rainbow.DomainDriven.Command
{
    public class CommandHandlerSelector : ICommandHandlerSelector
    {

        private ConcurrentDictionary<Type, Type> _cacheCommandExecutor = new ConcurrentDictionary<Type, Type>();

        public void Initialize(IEnumerable<Assembly> assemblys)
        {
            this.RegisterHandler(assemblys.SelectMany(p => p.GetTypes()));
        }

        public virtual Type FindHandlerType<TCommand>() where TCommand : ICommand
        {
            var HandlerType = default(Type);
            this._cacheCommandExecutor.TryGetValue(typeof(TCommand), out HandlerType);
            return HandlerType;
        }



        private void RegisterHandler(Type executorType)
        {
            var HandlerTypes = TypeHelper.GetGenericInterfaceTypes(executorType, typeof(ICommandHandler<>));

            foreach (var execType in HandlerTypes)
            {
                this._cacheCommandExecutor.TryAdd(execType.GetGenericArguments().FirstOrDefault(), executorType);
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

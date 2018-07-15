using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public class CommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Func<Type, IServiceProvider, object> _createInstance =
            (type, provider) => ActivatorUtilities.CreateInstance(provider, type, Type.EmptyTypes);

        private readonly ConcurrentDictionary<Type, object> _typeActivatorCache =
               new ConcurrentDictionary<Type, object>();

        public CommandHandlerFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public ICommandHandler<TCommand> Create<TCommand>(Type type) where TCommand : ICommand
        {
            return (ICommandHandler<TCommand>)_typeActivatorCache.GetOrAdd(type, t => _createInstance(t, _serviceProvider));
        }

    }
}

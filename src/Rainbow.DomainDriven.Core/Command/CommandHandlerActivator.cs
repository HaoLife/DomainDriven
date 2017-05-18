using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace Rainbow.DomainDriven.Command
{
    public class CommandHandlerActivator : ICommandHandlerActivator
    {

        private readonly IServiceProvider _serviceProvider;

        private readonly Func<Type, ObjectFactory> _createFactory =
            (type) => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes);

        private readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public CommandHandlerActivator(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public ICommandHandler<TCommand> Create<TCommand>(Type type) where TCommand : ICommand
        {
            var createFactory = _typeActivatorCache.GetOrAdd(type, _createFactory);
            return (ICommandHandler<TCommand>)createFactory(_serviceProvider, arguments: null);
        }

    }
}

using System;
using Rainbow.DomainDriven.Message;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.Event;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Rainbow.DomainDriven.Command
{
    public class ObjectExecutorFactory : ICommandExecutorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandHandler _commandHandler;
        private readonly ICommandExecutorContextFactory _commandExecutorContextFactory;
        private readonly IAggregateRootRepositoryContextFactory _aggregateRootRepositoryContextFactory;
        private readonly IEventExecutor _eventExecutor;


        private ConcurrentDictionary<MessageDescribe, ICommandExecutor> _cacheCommandExecutor = new ConcurrentDictionary<MessageDescribe, ICommandExecutor>();


        public ObjectExecutorFactory(
            IServiceProvider serviceProvider
            , IAggregateRootQuery aggregateRootQuery
            , IAggregateRootRepositoryContextFactory aggregateRootRepositoryContextFactory
            , ILoggerFactory loggerFactory
        )
        {
            this._serviceProvider = serviceProvider;
            var assemblyProvider = new AssemblyProvider();
            var commandHandlerActivator = new CommandHandlerActivator(serviceProvider);
            var commandHandlerSelector = new CommandHandlerSelector(assemblyProvider);

            var eventHandlerActivator = new EventHandlerActivator(serviceProvider);
            var eventHandlerSelector = new EventHandlerSelector(assemblyProvider);
            var eventHandler = new DomainEventHandler(eventHandlerSelector, eventHandlerActivator, loggerFactory);
            var eventExecutor = new EventExecutor(eventHandler);

            this._commandHandler = new CommandHandler(commandHandlerSelector, commandHandlerActivator);
            this._commandExecutorContextFactory = new CommandExecutorContextFactory(aggregateRootQuery);
            this._aggregateRootRepositoryContextFactory = aggregateRootRepositoryContextFactory;
            this._eventExecutor = eventExecutor;


        }
        public ICommandExecutor Create(MessageDescribe describe)
        {
            return _cacheCommandExecutor.GetOrAdd(describe, Build);
        }

        private ICommandExecutor Build(MessageDescribe describe)
        {
            var executor = new CommandExecutor(this._commandHandler, this._aggregateRootRepositoryContextFactory, this._commandExecutorContextFactory, this._eventExecutor);
            if (describe.Consistency == Consistency.Lose) return new AsyncCommandExecutor(executor);
            return executor;
        }
    }
}
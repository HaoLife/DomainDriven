using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Event;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class LocalQueueDomainHostBuilder : IDomainHostBuilder
    {
        private readonly IServiceCollection _service;

        public LocalQueueDomainHostBuilder(IServiceCollection service)
        {
            this._service = service;
        }

        public void Initialize()
        {
            this._service.AddOptions()
                .AddLogging();

            this._service.TryAdd(new ServiceCollection()
                .AddSingleton<IMessageProcessBuilder>(new MessageProcessBuilder())
                .AddSingleton<ICommandService, RingQueueCommandService>()
                .AddSingleton<ICommandExecutorProxyProvider, CommandExecutorProxyProvider>()
                .AddSingleton(typeof(CommandExecutorProxy<>))
                .AddSingleton<ICommandExecutor, RingQueueCommandExecutor>()
                .AddTransient<ICommandExecutorContext, RingQueueCommandExecutorContext>()
                .AddSingleton<ICommandExecutorContextFactory, CommandExecutorContextFactory>()
                .AddSingleton<ICommandHandlerActivator, CommandHandlerActivator>()
                .AddSingleton<IAggregateRootCache, AggregateRootCache>()
                .AddSingleton<IAggregateRootIndexCache, AggregateRootIndexCache>()
                .AddSingleton<IRollbackService, RollbackService>()

                .AddTransient<CommandExecutorHandler>()
                .AddTransient<CommandCacheHandler>()

                .AddSingleton<IEventService, RingQueueEventService>()
                .AddTransient<EventStoreHandler>()
                .AddTransient<SnapshotHandler>()
                .AddTransient<EventExecutorHandler>()

                .AddSingleton(typeof(ReplayEventProxy<>))
                .AddSingleton<IReplayEventProxyProvider, ReplayEventProxyProvider>()

                .AddSingleton<IEventExecutor, EventExecutor>()
                .AddSingleton<IEventHandlerActivator, EventHandlerActivator>()
                .AddTransient<IEventHandlerProxyProvider, EventHandlerProxyProvider>()
                .AddSingleton<IMessageListening>(new MessageListening())
            );
        }

        public IServiceCollection Services => _service;

        public IDomainHost Build()
        {
            return new LocalQueueDamianHost(_service);
        }

        public void ApplyServices(IDomainInitializeExtension ext)
        {
            ext.ApplyServices(this._service);
        }
    }
}
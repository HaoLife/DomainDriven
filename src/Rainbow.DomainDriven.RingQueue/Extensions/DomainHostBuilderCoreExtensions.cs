using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.RingQueue.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DomainHostBuilderCoreExtensions
    {

        internal static IDomainHostBuilder AddCore(this IDomainHostBuilder builder)
        {

            builder.Services.AddOptions()
                .AddLogging();

            builder.Services.TryAdd(new ServiceCollection()
                .AddSingleton<IMessageProcessBuilder>(new MessageProcessBuilder())
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
            return builder;
        }
    }
}
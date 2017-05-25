using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Host;
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
                .AddSingleton<ICommandExecutor, RingQueueCommandExecutor>()
                .AddSingleton<ICommandHandlerProxy, CommandHandlerProxy>()
                .AddSingleton<IAggregateRootSnapshot, AggregateRootSnapshot>()
                .AddTransient<ICommandExecutorContext, RingQueueCommandExecutorContext>()
                .AddSingleton<ICommandExecutorContextFactory, CommandExecutorContextFactory>()
                .AddSingleton<ICommandHandlerActivator, CommandHandlerActivator>()
                .AddSingleton<IAggregateRootCache, AggregateRootCache>()
                .AddSingleton<IAggregateRootIndexCache, AggregateRootIndexCache>()
                .AddSingleton<IReplayEventProxy, ReplayEventProxy>()

                .AddTransient<CommandExecutorHandler>()
                .AddTransient<CommandCacheHandler>()

                .AddTransient<EventRecallHandler>()
                .AddTransient<EventExecutorHandler>()

                .AddSingleton<IEventExecutor, EventExecutor>()
                .AddSingleton<IEventHandlerProxy, EventHandlerProxy>()
                .AddSingleton<IEventHandlerActivator, EventHandlerActivator>()
                .AddSingleton<IReplyMessageListening>(new ReplyMessageListening())
            );
            return builder;
        }
    }
}
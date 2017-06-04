using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Domain;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class CoreServiceCollectionExtensions
    {
        
        public static IServiceCollection AddRingQueueCore(this IServiceCollection services)
        {

            services.TryAdd(new ServiceCollection()
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
                
                .AddSingleton<ICommandHandlerSelector, CommandHandlerSelector>()
                .AddSingleton<IEventHandlerSelector, EventHandlerSelector>()
                .AddSingleton<IDomainTypeProvider, DomainTypeProvider>()
                .AddSingleton<IEventSourcingProcess, EventSourcingProcess>()
            );
            
            return services;
        }
    }
}
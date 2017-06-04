using Rainbow.DomainDriven;
using Rainbow.DomainDriven.Host;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.TryAdd(new ServiceCollection()
                .AddSingleton<IAssemblyProvider, AssemblyProvider>()
                .AddSingleton<ICommandService, CommandService>()
                .AddSingleton<ICommandExecutor, CommandExecutor>()
                .AddSingleton<ICommandHandlerProxy, CommandHandlerProxy>()
                .AddSingleton<ICommandExecutorContextFactory, CommandExecutorContextFactory>()
                .AddSingleton<ICommandHandlerActivator, CommandHandlerActivator>()
                .AddSingleton<IEventHandlerActivator, EventHandlerActivator>()
                .AddSingleton<IEventExecutor, EventExecutor>()
                .AddSingleton<IEventHandlerProxy, EventHandlerProxy>()
                .AddSingleton<ICommandHandlerSelector, CommandHandlerSelector>()
                .AddSingleton<IEventHandlerSelector, EventHandlerSelector>()
                .AddSingleton<IDomainTypeProvider, DomainTypeProvider>()
                .AddTransient<ICommandExecutorContext, CommandExecutorContext>()
                .AddSingleton<IDomainHost>(new DomainHost(services))
            );


            return services;
        }


        public static IServiceCollection AddDomainService(this IServiceCollection services)
        {
            var builder = new DomainServiceBuilder(services);
            builder.Build();
            return services;
        }

    }
}

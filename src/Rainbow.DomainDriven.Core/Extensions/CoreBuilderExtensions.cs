using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Extensions
{
    public static class CoreBuilderExtensions
    {
        public static IDomainBuilder AddDomainCore(this IDomainBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.TryAdd(new ServiceCollection()
                .AddSingleton<IAssemblyProvider, AssemblyProvider>()
                .AddSingleton<IEventRebuildHandler, EventRebuildHandler>()
                .AddSingleton<ICommandRegister, AutoCommandRegister>()
                .AddSingleton<ICommandHandlerFactory, CommandHandlerFactory>()
                .AddSingleton<IEventRegister, AutoEventRegister>()
                .AddSingleton<IEventHandlerFactory, EventHandlerFactory>()
                .AddSingleton<IAggregateRootRebuilder, AggregateRootRebuilder>()
                .AddSingleton<IEventRebuildInitializer, EventRebuildInitializer>()
                .AddSingleton<IAggregateRootValidator, AggregateRootValidator>()
                );

            return builder;
        }

    }
}

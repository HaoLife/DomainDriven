using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Framework;
using Rainbow.DomainDriven.RingQueue.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.Extensions;
using Rainbow.DomainDriven.Domain;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RingDomainBuilderExtensions
    {
        public static IDomainBuilder AddRing(this IDomainBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddLogging();
            builder.Services.AddMemoryCache();

            builder.AddDomainCore();
            builder.AddRingOptions(configuration);

            builder.Services.TryAdd(new ServiceCollection()
                .AddSingleton<ICommandBus, RingCommandBus>()
                .AddSingleton<IAggregateRootRebuilder, AggregateRootRebuilder>()
                );
            //IEventBus


            return builder;
        }
        internal static IDomainBuilder AddRingOptions(this IDomainBuilder builder, IConfiguration configuration)
        {
            builder.Services.TryAdd(new ServiceCollection()
                .AddSingleton<IConfigureOptions<RingOptions>>(new RingConfigureOptions(configuration))
                .AddSingleton<IOptionsChangeTokenSource<RingOptions>>(new ConfigurationChangeTokenSource<RingOptions>(configuration))

                );

            return builder;
        }
    }
}

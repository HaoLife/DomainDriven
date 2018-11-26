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
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.Event;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RingDomainBuilderExtensions
    {
        public static IDomainBuilder AddRing(this IDomainBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddMemoryCache();

            builder.AddDomainCore();
            builder.AddRingOptions(configuration);

            builder.Services.TryAdd(new ServiceCollection()
                .AddSingleton<ICommandBus, RingCommandBus>()
                .AddSingleton<IEventBus, RingEventBus>()
                .AddSingleton<IContextCache, RingContextMemoryCache>()
                .AddSingleton<ISnapshootCache, SnapshootMemoryCache>()
                .AddSingleton<IEventHandleSubject, EventHandleSubject>()
                .AddSingleton<IRingBufferProcess, RingBufferProcess>()
                .AddSingleton<IDomainLauncher, RingQueueDomainLauncher>()
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

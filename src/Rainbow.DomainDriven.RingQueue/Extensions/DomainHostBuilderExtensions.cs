using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DomainHostBuilderExtensions
    {

        public static IDomainHostBuilder UseCommandMapping<TCommandMappingProvider>(this IDomainHostBuilder builder)
            where TCommandMappingProvider : class, ICommandMappingProvider
        {
            builder.Services
                .AddSingleton<ICommandMappingProvider, TCommandMappingProvider>()
                .AddSingleton<CommandCacheHandler>();

            return builder;
        }

        public static IDomainHostBuilder UseEventSourcing(this IDomainHostBuilder builder)
        {
            builder.Services
                .AddSingleton<IEventSourcingProcess, EventSourcingProcess>();
            return builder;
        }

    }
}
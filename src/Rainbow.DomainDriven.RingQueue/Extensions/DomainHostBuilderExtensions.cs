using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Command;

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

    }
}
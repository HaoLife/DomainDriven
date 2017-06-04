using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Host;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddLocalQueueDomain(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRingQueueCore();
            services.TryAdd(new ServiceCollection()
                .AddSingleton<ICommandService, RingQueueCommandService>());

            services.AddSingleton<IDomainHost>(new LocalQueueDamianHost(services, configuration));
            return services;
        }

        public static IServiceCollection AddLocalMultiQueueDomain(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRingQueueCore();
            services.TryAdd(new ServiceCollection()
                .AddSingleton<ICommandService, RingMultiQueueCommandService>());


            services.AddSingleton<IDomainHost>(new LocalQueueDamianHost(services, configuration));

            return services;
        }


        public static IServiceCollection AddCommandMapping<TCommandMappingProvider>(this IServiceCollection services)
            where TCommandMappingProvider : class, ICommandMappingProvider
        {
            services
                .AddSingleton<ICommandMappingProvider, TCommandMappingProvider>()
                .AddSingleton<CommandCacheHandler>();

            return services;
        }

    }
}
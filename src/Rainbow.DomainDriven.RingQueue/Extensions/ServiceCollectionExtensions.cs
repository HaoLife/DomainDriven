using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Host;
using Rainbow.DomainDriven.RingQueue.Message;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalQueueDomain(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAdd(new ServiceCollection()
                .AddSingleton<IMessageProcess>(new MessageProcess())
                .AddSingleton<IDomainHost>(new LocalQueueDamianHost(services, configuration))
                .AddSingleton<ICommandService, CommandService>()
                );
            return services;
        }

        public static IServiceCollection AddLocalMultiQueueDomain(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }



    }
}
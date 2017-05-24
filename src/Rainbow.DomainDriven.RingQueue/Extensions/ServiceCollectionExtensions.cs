using Microsoft.Extensions.Configuration;
using Rainbow.DomainDriven.DomainExtensions;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.RingQueue;
using Rainbow.DomainDriven.RingQueue.Host;
using Rainbow.DomainDriven.RingQueue.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IDomainHostBuilder UseLocalQueueDomain(this IServiceCollection service, IConfiguration configuration)
        {
            service.Configure<RiginQueueOptions>(configuration);
            
            var builder = new LocalQueueDomainHostBuilder(service);
            builder.Initialize();
            builder.ApplyServices(new SelectorInitializeExtension());
            builder.ApplyServices(new DomainTypeProviderExtension());
            return builder;
        }


        public static IDomainHostBuilder UseLocalMultiQueueDomain(this IServiceCollection service, IConfiguration configuration)
        {
            service.Configure<RiginQueueOptions>(configuration);
            
            var builder = new LocalMultiQueueDomainHostBuilder(service);
            builder.Initialize();
            builder.ApplyServices(new SelectorInitializeExtension());
            builder.ApplyServices(new DomainTypeProviderExtension());
            return builder;
        }
    }
}
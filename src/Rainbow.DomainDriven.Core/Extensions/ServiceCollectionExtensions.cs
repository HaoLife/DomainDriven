using Rainbow.DomainDriven;
using Rainbow.DomainDriven.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IDomainHostBuilder UseDefaultDomain(this IServiceCollection service)
        {
            var builder = new DomainHostBuilder(service);
            builder.Initialize();
            builder.ApplyServices(new SelectorInitializeExtension());
            builder.ApplyServices(new EventHandlerInitializeExtension());
            builder.ApplyServices(new DomainTypeSearchExtension());
            return builder;
        }
    }
}

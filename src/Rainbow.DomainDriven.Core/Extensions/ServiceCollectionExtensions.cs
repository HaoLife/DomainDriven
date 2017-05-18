using Rainbow.DomainDriven;
using Rainbow.DomainDriven.Host;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IDomainHostBuilder UseDefaultDomain(this IServiceCollection service)
        {
            var builder = new DomainHostBuilder(service);
            return builder;
        }
    }
}

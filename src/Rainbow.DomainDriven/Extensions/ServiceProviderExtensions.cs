using System;
using Rainbow.DomainDriven.Host;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        
        public static IServiceProvider UseDomain(this IServiceProvider provider)
        {
            var domainHost = provider.GetRequiredService<IDomainHost>();
            domainHost.Start();
            return provider;
        }


    }
}
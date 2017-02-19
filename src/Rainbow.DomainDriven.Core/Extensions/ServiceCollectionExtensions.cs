using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IDomainHostBuilder UseDefaultDomain(this IServiceCollection service)
        {
            var builder = new DomainHostBuilder(service);
            builder.Initialize();
            return builder;
        }
    }
}

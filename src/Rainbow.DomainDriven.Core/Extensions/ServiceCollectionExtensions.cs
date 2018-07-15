using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services, Action<IDomainBuilder> configure)
        {
            configure(new DomainBuilder(services));
            return services;
        }
    }
}

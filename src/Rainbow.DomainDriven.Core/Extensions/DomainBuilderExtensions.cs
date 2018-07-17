using Rainbow.DomainDriven.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Domain;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DomainBuilderExtensions
    {

        public static IDomainBuilder AddDomainService(this IDomainBuilder builder)
        {
            builder.Services.TryAddSingleton<IDomainServiceProvider, IDomainServiceProvider>();
            return builder;
        }

        public static IDomainBuilder AddMapping<TMapping>(this IDomainBuilder builder) where TMapping : class, ICommandMappingProvider
        {
            builder.Services.TryAddSingleton<ICommandMappingProvider, TMapping>();
            return builder;
        }

    }
}

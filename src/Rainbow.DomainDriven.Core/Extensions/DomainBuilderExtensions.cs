using Rainbow.DomainDriven.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Domain;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DomainBuilderExtensions
    {

        public static IDomainBuilder AddDomainService(this IDomainBuilder builder)
        {
            var domainServiceBuilder = new DomainServiceBuilder(builder.Services);
            domainServiceBuilder.Build();
            return builder;
        }

        public static IDomainBuilder AddMapping<TMapping>(this IDomainBuilder builder) where TMapping : class, ICommandMappingProvider
        {
            builder.Services.TryAddSingleton<ICommandMappingProvider, TMapping>();
            return builder;
        }


        public static IDomainBuilder AddMixedMapping(this IDomainBuilder builder, Action<MixedCommandMappingProviderBuilder> action)
        {
            var mappingBuilder = new MixedCommandMappingProviderBuilder();
            action(mappingBuilder);
            builder.Services.TryAddSingleton<ICommandMappingProvider>(mappingBuilder.Build);

            return builder;
        }

        public static IDomainBuilder AddMixedMapping(this IDomainBuilder builder)
        {
            var mappingBuilder = new MixedCommandMappingProviderBuilder();
            mappingBuilder.AddAutoMapping();
            builder.Services.TryAddSingleton<ICommandMappingProvider>(mappingBuilder.Build);

            return builder;
        }
    }
}

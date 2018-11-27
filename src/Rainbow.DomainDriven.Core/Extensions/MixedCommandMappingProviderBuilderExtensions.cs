using Rainbow.DomainDriven.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MixedCommandMappingProviderBuilderExtensions
    {
        public static MixedCommandMappingProviderBuilder AddMapping<TMapping>(this MixedCommandMappingProviderBuilder builder) where TMapping : class, ICommandMappingProvider
        {
            builder.AddProvider<TMapping>();
            return builder;
        }

        public static MixedCommandMappingProviderBuilder AddAutoMapping(this MixedCommandMappingProviderBuilder builder)
        {
            builder.AddProvider<AutoCommandMappingProvider>();
            return builder;
        }
    }
}

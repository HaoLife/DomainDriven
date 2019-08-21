using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public class MixedCommandMappingProviderBuilder
    {
        private List<Type> providers = new List<Type>();

        public void AddProvider<TProvider>() where TProvider : ICommandMappingProvider
        {
            providers.Add(typeof(TProvider));
        }

        public void Clear()
        {
            providers.Clear();
        }


        public MixedCommandMappingProvider Build(IServiceProvider provider)
        {
            List<ICommandMappingProvider> commandMappingProviders = new List<ICommandMappingProvider>();
            foreach (var type in providers)
            {
                var commandMappingProvider = (ICommandMappingProvider)ActivatorUtilities.CreateInstance(provider, type, Type.EmptyTypes);

                commandMappingProviders.Add(commandMappingProvider);
            }

            return new MixedCommandMappingProvider(commandMappingProviders);
        }
    }
}

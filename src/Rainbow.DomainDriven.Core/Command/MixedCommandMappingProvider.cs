using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public class MixedCommandMappingProvider : ICommandMappingProvider
    {
        private readonly IEnumerable<ICommandMappingProvider> _commandMappingProviders;

        public MixedCommandMappingProvider(IEnumerable<ICommandMappingProvider> commandMappingProviders)
        {
            this._commandMappingProviders = commandMappingProviders;
        }


        public Dictionary<Guid, Type> Find(ICommand cmd)
        {
            Dictionary<Guid, Type> dict = new Dictionary<Guid, Type>();
            foreach (var item in _commandMappingProviders)
            {
                foreach (var kv in item.Find(cmd))
                {
                    if (dict.ContainsKey(kv.Key)) continue;
                    dict.Add(kv.Key, kv.Value);
                }
            }
            return dict;
        }
    }
}

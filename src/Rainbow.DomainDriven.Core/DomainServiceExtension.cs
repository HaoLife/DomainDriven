using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven
{
    public class DomainServiceExtension : IDomainOptionsExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
            var dependencyContext = DependencyContext.Load(Assembly.GetEntryAssembly());
            IEnumerable<Assembly> assemblys = dependencyContext.RuntimeLibraries
                .SelectMany(p => p.GetDefaultAssemblyNames(dependencyContext))
                .Select(Assembly.Load);
            var types = assemblys.SelectMany(p => p.GetTypes());

            foreach (var type in types)
            {
                if (!typeof(IDomainService).GetTypeInfo().IsAssignableFrom(type)) continue;
                if (!type.GetTypeInfo().IsClass) continue;
                services.AddSingleton(type);
            }

        }
    }
}
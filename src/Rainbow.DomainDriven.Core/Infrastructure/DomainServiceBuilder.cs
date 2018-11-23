using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Infrastructure
{
    public class DomainServiceBuilder
    {
        private readonly IServiceCollection _services;
        public DomainServiceBuilder(IServiceCollection services)
        {
            this._services = services;
        }

        public void Build()
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
                this._services.AddSingleton(type);
            }

        }
    }
}

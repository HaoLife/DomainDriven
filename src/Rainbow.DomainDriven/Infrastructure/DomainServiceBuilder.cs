using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Rainbow.DomainDriven.Domain;
using System.Linq;

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
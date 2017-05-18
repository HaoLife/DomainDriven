using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Rainbow.DomainDriven.Infrastructure;

namespace Rainbow.DomainDriven.DomainExtensions
{
    public class DomainTypeProviderExtension : IDomainInitializeExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
            var dependencyContext = DependencyContext.Load(Assembly.GetEntryAssembly());
            IEnumerable<Assembly> assemblys = dependencyContext.RuntimeLibraries
                .SelectMany(p => p.GetDefaultAssemblyNames(dependencyContext))
                .Select(Assembly.Load);

            var domainTypeSearch = new DomainTypeProvider();
            domainTypeSearch.Initialize(assemblys);

            
            services.AddSingleton<IDomainTypeProvider>(domainTypeSearch);
        }
    }
}
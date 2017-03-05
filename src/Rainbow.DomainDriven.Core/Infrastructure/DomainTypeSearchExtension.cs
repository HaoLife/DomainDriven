using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace Rainbow.DomainDriven.Infrastructure
{
    public class DomainTypeSearchExtension : IDomainInitializeExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
            var dependencyContext = DependencyContext.Load(Assembly.GetEntryAssembly());
            IEnumerable<Assembly> assemblys = dependencyContext.RuntimeLibraries
                .SelectMany(p => p.GetDefaultAssemblyNames(dependencyContext))
                .Select(Assembly.Load);

            var domainTypeSearch = new DomainTypeSearch();
            domainTypeSearch.Initialize(assemblys);

            
            services.AddSingleton<IDomainTypeSearch>(domainTypeSearch);
        }
    }
}
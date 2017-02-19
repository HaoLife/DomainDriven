using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Mongo.Repository;

namespace Rainbow.DomainDriven.Mongo
{
    public class MongoInitializeExtension : IDomainOptionsExtension
    {
        public void ApplyServices(IServiceCollection services)
        {

            var dependencyContext = DependencyContext.Load(Assembly.GetEntryAssembly());
            IEnumerable<Assembly> assemblys = dependencyContext.RuntimeLibraries
                .SelectMany(p => p.GetDefaultAssemblyNames(dependencyContext))
                .Select(Assembly.Load);

            var provider = services.BuildServiceProvider();
            var aggregateRootRepositoryProvider = new AggregateRootRepositoryProvider(services, provider);
            aggregateRootRepositoryProvider.Initialize(assemblys);
            
            services.AddSingleton<IAggregateRootRepositoryProvider>(aggregateRootRepositoryProvider);
        }
    }
}
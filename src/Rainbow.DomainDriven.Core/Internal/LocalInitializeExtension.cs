using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Internal
{
    internal class LocalInitializeExtension : IDomainOptionsExtension
    {
        public void ApplyServices(IServiceCollection services)
        {

            services.AddSingleton(typeof(EventHandlerProxy<>));
            services.AddSingleton<IEventHandlerProxyProvider, EventHandlerProxyProvider>();


            // var dependencyContext = DependencyContext.Load(Assembly.GetEntryAssembly());
            // IEnumerable<Assembly> assemblys = dependencyContext.RuntimeLibraries
            //     .SelectMany(p => p.GetDefaultAssemblyNames(dependencyContext))
            //     .Select(Assembly.Load);


            // var eventHandlerProxyProvider = new EventHandlerProxyProvider(services.BuildServiceProvider());
            // eventHandlerProxyProvider.Initialize(assemblys);
            // services.AddSingleton<IEventHandlerProxyProvider>(eventHandlerProxyProvider);
        }
    }
}
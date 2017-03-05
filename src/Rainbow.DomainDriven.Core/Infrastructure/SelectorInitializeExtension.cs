using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Infrastructure
{
    public class SelectorInitializeExtension : IDomainInitializeExtension
    {
        public void ApplyServices(IServiceCollection services)
        {

            var dependencyContext = DependencyContext.Load(Assembly.GetEntryAssembly());
            IEnumerable<Assembly> assemblys = dependencyContext.RuntimeLibraries
                .SelectMany(p => p.GetDefaultAssemblyNames(dependencyContext))
                .Select(Assembly.Load);

            var commandHandlerSelector = new CommandHandlerSelector();
            commandHandlerSelector.Initialize(assemblys);

            var eventHandlerSelector = new EventHandlerSelector();
            eventHandlerSelector.Initialize(assemblys);
            
            services.AddSingleton<ICommandHandlerSelector>(commandHandlerSelector);
            services.AddSingleton<IEventHandlerSelector>(eventHandlerSelector);
        }
    }
}
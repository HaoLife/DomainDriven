using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.DomainExtensions;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Host
{
    public class DomainHostBuilder : IDomainHostBuilder
    {
        private readonly IServiceCollection _service;

        public DomainHostBuilder(IServiceCollection service)
        {
            this._service = service;
            Initialize();
        }

        private void Initialize()
        {
            this._service.AddOptions();
            this._service.AddLogging();

            this._service.TryAdd(new ServiceCollection()
                .AddSingleton<ICommandService, CommandService>()
                .AddSingleton<ICommandExecutor, CommandExecutor>()
                .AddSingleton<ICommandExecutorContextFactory, CommandExecutorContextFactory>()
                .AddSingleton<ICommandHandlerActivator, CommandHandlerActivator>()
                .AddSingleton<IEventHandlerActivator, EventHandlerActivator>()
                .AddSingleton<IEventExecutor, EventExecutor>()
                .AddSingleton<IEventHandlerProxy, EventHandlerProxy>()
                .AddTransient<ICommandExecutorContext, CommandExecutorContext>()
            );
            this.ApplyServices(new SelectorInitializeExtension());
            this.ApplyServices(new DomainTypeProviderExtension());
        }

        public IServiceCollection Services => _service;

        public IDomainHost Build()
        {
            return new DamianHost(_service);
        }

        public void ApplyServices(IDomainInitializeExtension ext)
        {
            ext.ApplyServices(this._service);
        }
    }
}

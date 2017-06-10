using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Repository;
using Microsoft.Extensions.Logging;

namespace Rainbow.DomainDriven.Host
{
    public sealed class DomainHost : IDomainHost
    {
        private readonly IServiceCollection _services;
        private IServiceProvider _provider;

        private ICommandExecutorFactory _commandExecutorFactory;

        public ICommandExecutorFactory Factory => this._commandExecutorFactory;

        public DomainHost(IServiceCollection services)
        {
            this._services = services;
        }

        private void InitializeDatabase()
        {
            var initalizer = this._provider?.GetService<IDatabaseInitializer>();
            if (initalizer != null)
            {
                initalizer.Initialize(this._provider);
            }
        }
        private void Initialize()
        {
            var aggregateRootQuery = this._provider.GetRequiredService<IAggregateRootQuery>();
            var aggregateRootRepositoryContextFactory = this._provider.GetRequiredService<IAggregateRootRepositoryContextFactory>();
            var loggerFactory = this._provider.GetRequiredService<ILoggerFactory>();

            this._commandExecutorFactory = new ObjectExecutorFactory(this._provider, aggregateRootQuery, aggregateRootRepositoryContextFactory, loggerFactory);
        }

        public void Start()
        {
            this._provider = this._services.BuildServiceProvider();
            this.InitializeDatabase();
            this.Initialize();
        }
    }
}

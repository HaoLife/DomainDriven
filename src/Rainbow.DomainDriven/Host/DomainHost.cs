using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Infrastructure;

namespace Rainbow.DomainDriven.Host
{
    public class DomainHost : IDomainHost
    {
        protected readonly IServiceCollection _services;
        protected IServiceProvider _provider;

        public DomainHost(IServiceCollection services)
        {
            this._services = services;
        }

        protected virtual void InitializeDatabase()
        {
            var initalizer = this._provider?.GetService<IDatabaseInitializer>();
            if (initalizer != null)
            {
                initalizer.Initialize(this._provider);
            }
        }

        public virtual void Start()
        {
            this._provider = this._services.BuildServiceProvider();
            this.InitializeDatabase();
        }
    }
}

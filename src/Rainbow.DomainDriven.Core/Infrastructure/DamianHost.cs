using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Options;

namespace Rainbow.DomainDriven.Infrastructure
{
    public class DamianHost : IDomainHost
    {
        private readonly IServiceCollection _service;

        public DamianHost(IServiceCollection service)
        {
            this._service = service;
        }

        public void Start()
        {
            
        }
    }
}

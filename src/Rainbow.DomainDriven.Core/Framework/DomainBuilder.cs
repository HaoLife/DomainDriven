using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Rainbow.DomainDriven.Framework
{
    public class DomainBuilder : IDomainBuilder
    {
        public DomainBuilder(IServiceCollection services)
        {
            this.Services = services;

        }

        public IServiceCollection Services { get; private set; }
    }
}

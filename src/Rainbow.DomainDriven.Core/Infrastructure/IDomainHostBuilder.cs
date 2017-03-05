using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Infrastructure
{
    public interface IDomainHostBuilder
    {
        IServiceCollection Services { get; }
        void ApplyServices(IDomainInitializeExtension ext);

        IDomainHost Build();
    }
}

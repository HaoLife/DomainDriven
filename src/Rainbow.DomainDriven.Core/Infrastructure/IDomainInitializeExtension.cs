using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rainbow.DomainDriven.Infrastructure
{
    public interface IDomainInitializeExtension
    {
        void ApplyServices(IServiceCollection services);
    }
}
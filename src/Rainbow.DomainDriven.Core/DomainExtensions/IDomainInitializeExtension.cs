using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rainbow.DomainDriven.DomainExtensions
{
    public interface IDomainInitializeExtension
    {
        void ApplyServices(IServiceCollection services);
    }
}
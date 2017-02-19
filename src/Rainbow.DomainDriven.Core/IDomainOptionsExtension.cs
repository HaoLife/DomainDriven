using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rainbow.DomainDriven
{
    public interface IDomainOptionsExtension
    {
        void ApplyServices(IServiceCollection services);
    }
}
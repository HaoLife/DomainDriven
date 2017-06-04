using System;

namespace Rainbow.DomainDriven.Infrastructure
{
    public interface IDatabaseInitializer
    {
        void Initialize(IServiceProvider provider);
    }
}
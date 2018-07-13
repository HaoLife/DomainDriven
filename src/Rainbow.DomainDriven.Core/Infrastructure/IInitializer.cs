using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Infrastructure
{
    public interface IInitializer
    {
        void Initialize(IServiceProvider provider);
    }
}

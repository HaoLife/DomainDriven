using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Framework
{
    public interface IDomainBuilder
    {
        IServiceCollection Services { get; }
    }
}

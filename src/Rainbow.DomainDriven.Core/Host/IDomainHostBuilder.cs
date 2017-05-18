﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.DomainExtensions;

namespace Rainbow.DomainDriven.Host
{
    public interface IDomainHostBuilder
    {
        IServiceCollection Services { get; }
        void ApplyServices(IDomainInitializeExtension ext);

        IDomainHost Build();
    }
}
using System;
using System.Collections.Generic;

namespace Rainbow.DomainDriven.Infrastructure
{
    public interface IDomainTypeSearch
    {
        Type GetType(string name);
    }
}
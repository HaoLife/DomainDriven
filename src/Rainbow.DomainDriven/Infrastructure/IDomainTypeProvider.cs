using System;
using System.Collections.Generic;

namespace Rainbow.DomainDriven.Infrastructure
{
    public interface IDomainTypeProvider
    {
         
        Type GetType(string name);
        IEnumerable<Type> Events { get; }
        IEnumerable<Type> AggregateRoots { get; }
        IEnumerable<Type> Commands { get; }
    }
}
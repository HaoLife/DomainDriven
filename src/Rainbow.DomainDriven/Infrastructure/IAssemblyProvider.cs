using System.Collections.Generic;
using System.Reflection;

namespace Rainbow.DomainDriven.Infrastructure
{
    public interface IAssemblyProvider
    {
        IEnumerable<Assembly> Assemblys { get; }
    }
}
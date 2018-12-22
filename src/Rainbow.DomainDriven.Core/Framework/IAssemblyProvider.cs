using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rainbow.DomainDriven.Framework
{
    public interface IAssemblyProvider
    {
        IEnumerable<Assembly> Assemblys { get; }
    }
}

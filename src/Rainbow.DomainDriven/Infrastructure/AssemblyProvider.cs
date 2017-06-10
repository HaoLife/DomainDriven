using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;

namespace Rainbow.DomainDriven.Infrastructure
{
    public class AssemblyProvider : IAssemblyProvider
    {
        private readonly List<Assembly> _assemblys;

        public AssemblyProvider()
        {
            var dependencyContext = DependencyContext.Load(Assembly.GetEntryAssembly());
            IEnumerable<Assembly> assemblys = dependencyContext.RuntimeLibraries
                .SelectMany(p => p.GetDefaultAssemblyNames(dependencyContext))
                .Select(Assembly.Load);
            this._assemblys = assemblys.ToList();
        }

        public IEnumerable<Assembly> Assemblys => this._assemblys;
    }
}
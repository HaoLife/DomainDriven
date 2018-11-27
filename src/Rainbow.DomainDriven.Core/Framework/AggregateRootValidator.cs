using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Rainbow.DomainDriven.Domain;
using System.Reflection;

namespace Rainbow.DomainDriven.Framework
{
    public class AggregateRootValidator : IAggregateRootValidator
    {
        private IAssemblyProvider _assemblyProvider;

        public AggregateRootValidator(IAssemblyProvider assemblyProvider)
        {
            _assemblyProvider = assemblyProvider;
        }

        public void Validate()
        {
            var roots = _assemblyProvider.Assemblys
                .SelectMany(a => a.GetTypes())
                .Where(a => IsAggregateRoot(a));

            var empty = new Type[0];

            foreach (var root in roots)
            {
                var ctor = root.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, empty, null);

                if (ctor == null)
                {
                    throw new Exception($"领域模型[{root.Name}]没有无参的构造方法");
                }

            }


        }



        protected virtual bool IsAggregateRoot(Type type)
        {
            if (!typeof(IAggregateRoot).IsAssignableFrom(type) || !type.IsClass) return false;
            var typeinfo = type.GetTypeInfo();
            return typeinfo.IsClass && !typeinfo.IsAbstract;

        }

    }
}

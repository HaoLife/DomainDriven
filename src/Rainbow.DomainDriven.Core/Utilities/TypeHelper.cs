using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace Rainbow.DomainDriven.Utilities
{
    public static class TypeHelper
    {

        public static IEnumerable<Type> GetGenericInterfaceTypes(Type concreteType, Type genericType)
        {
            foreach (var @interface in concreteType.GetTypeInfo().GetInterfaces())
            {
                if (!@interface.GetTypeInfo().IsGenericType) continue;

                if (@interface.GetGenericTypeDefinition() == genericType)
                {
                    yield return @interface;
                }
            }
        }

    }
}

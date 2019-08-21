using Rainbow.DomainDriven.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Rainbow.DomainDriven.Domain;
using System.Reflection;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Rainbow.DomainDriven.Command
{
    public class AutoCommandMappingProvider : ICommandMappingProvider
    {
        private IAssemblyProvider _assemblyProvider;
        private ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> _mappings = new ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>>();

        protected Dictionary<string, Type> RootTypes { get; private set; }
        protected ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> Mappings => _mappings;


        public AutoCommandMappingProvider(IAssemblyProvider assemblyProvider)
        {
            this._assemblyProvider = assemblyProvider;
            Initialize();
        }


        public void Initialize()
        {
            var types = _assemblyProvider.Assemblys.SelectMany(p => p.GetTypes()).ToArray();
            RootTypes = types.Where(a => IsAggregateRoot(a)).ToDictionary(a => a.Name);
            var commands = types.Where(a => IsCommand(a)).ToList();


            foreach (var item in commands)
            {
                var mappings = item.GetTypeInfo().GetProperties().Where(a => IsCommandMapping(a));
                foreach (var map in mappings)
                {
                    var rootType = RootTypes[map.Name.Substring(0, map.Name.Length - 2)];
                    RegisterCommand(item, rootType, map);
                }
            }



        }

        protected virtual Delegate GenerateExpression(Type commandType, PropertyInfo mappingProperty)
        {
            var param_obj = Expression.Parameter(commandType);
            var param_val = Expression.Parameter(typeof(object));

            //转成真实类型，防止Dynamic类型转换成object
            var body_obj = Expression.Convert(param_obj, commandType);

            var body = Expression.Property(body_obj, mappingProperty);
            var getValue = Expression.Lambda(body, param_obj).Compile();

            return getValue;
        }

        protected virtual void RegisterCommand(Type commandType, Type aggrType, PropertyInfo mappingProperty)
        {
            var handle = GenerateExpression(commandType, mappingProperty);

            var cache = Mappings.GetOrAdd(commandType, new ConcurrentDictionary<Delegate, Type>());

            cache.TryAdd(handle, aggrType);
        }


        protected virtual bool IsIgnoreMapping(PropertyInfo info)
        {
            var prefix = "Create";
            var suffix = "Command";
            var endLength = info.DeclaringType.Name.EndsWith(suffix) ? suffix.Length : 0;

            var isCreate = info.DeclaringType.Name.StartsWith(prefix);
            var name = info.DeclaringType.Name.Substring(6, info.DeclaringType.Name.Length - prefix.Length - endLength);
            if (info.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        protected virtual bool IsCommandMapping(PropertyInfo info)
        {
            return info.PropertyType.Equals(typeof(Guid))
                && info.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
                && RootTypes.ContainsKey(info.Name.Substring(0, info.Name.Length - 2))
                && !IsIgnoreMapping(info);
        }


        protected virtual bool IsAggregateRoot(Type type)
        {
            if (!typeof(IAggregateRoot).IsAssignableFrom(type) || !type.IsClass) return false;
            var typeinfo = type.GetTypeInfo();
            return typeinfo.IsClass && !typeinfo.IsAbstract;

        }


        protected virtual bool IsCommand(Type type)
        {
            if (!typeof(ICommand).IsAssignableFrom(type) || !type.IsClass) return false;
            var typeinfo = type.GetTypeInfo();
            return typeinfo.IsClass && !typeinfo.IsAbstract;

        }

        public Dictionary<Guid, Type> Find(ICommand cmd)
        {
            ConcurrentDictionary<Delegate, Type> maps;
            _mappings.TryGetValue(cmd.GetType(), out maps);
            if (maps == null) return new Dictionary<Guid, Type>();

            Dictionary<Guid, Type> dicts = new Dictionary<Guid, Type>();

            foreach (var item in maps)
            {
                var keyOrKeyList = item.Key.DynamicInvoke(cmd);
                if (keyOrKeyList is Guid g)
                {
                    if (dicts.ContainsKey(g)) continue;
                    dicts.Add(g, item.Value);
                }
            }

            return dicts;
        }
    }
}

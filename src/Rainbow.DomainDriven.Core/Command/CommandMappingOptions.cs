using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Rainbow.DomainDriven.Command
{
    public class CommandMappingOptions : ICommandMapper
    {

        private ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> _mappings = new ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>>();
        private ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> _validates = new ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>>();

        private Dictionary<Guid, Type> Find(ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> source, object cmd)
        {
            ConcurrentDictionary<Delegate, Type> maps;
            if (!source.TryGetValue(cmd.GetType(), out maps))
                return new Dictionary<Guid, Type>();
            return maps.ToDictionary(p => (Guid)p.Key.DynamicInvoke(cmd), p => p.Value);
        }

        public Dictionary<Guid, Type> FindMap(object cmd)
        {
            return this.Find(this._mappings, cmd);
        }
        public Dictionary<Guid, Type> FindValidate(object cmd)
        {
            return this.Find(this._validates, cmd);
        }

        public void Map<TCommand>(Expression<Func<TCommand, Guid>> key, Type type)
        {
            var mapping = _mappings.GetOrAdd(typeof(TCommand), new ConcurrentDictionary<Delegate, Type>());
            mapping.AddOrUpdate(key.Compile(), type, (a, b) => type);
        }

        public void Validate<TCommand>(Expression<Func<TCommand, Guid>> key, Type type)
        {
            var validate = _validates.GetOrAdd(typeof(TCommand), new ConcurrentDictionary<Delegate, Type>());
            validate.AddOrUpdate(key.Compile(), type, (a, b) => type);
        }
    }
}
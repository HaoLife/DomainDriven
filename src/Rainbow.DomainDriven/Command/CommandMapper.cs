using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Command
{
    public class CommandMapper : ICommandMapper
    {

        private ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> _mappings = new ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>>();

        public void Map<TCommand, TAggregateRoot>(Expression<Func<TCommand, Guid>> key)
            where TCommand : ICommand
            where TAggregateRoot : IAggregateRoot
        {
            var type = typeof(TAggregateRoot);
            var mapping = _mappings.GetOrAdd(typeof(TCommand), new ConcurrentDictionary<Delegate, Type>());
            mapping.AddOrUpdate(key.Compile(), type, (a, b) => type);
        }


        public Dictionary<Guid, Type> FindMap(ICommand cmd)
        {
            return this.Find(this._mappings, cmd);
        }
        private Dictionary<Guid, Type> Find(ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> source, ICommand cmd)
        {
            ConcurrentDictionary<Delegate, Type> maps;
            if (!source.TryGetValue(cmd.GetType(), out maps))
                return new Dictionary<Guid, Type>();
            return maps.ToDictionary(p => (Guid)p.Key.DynamicInvoke(cmd), p => p.Value);
        }


    }
}
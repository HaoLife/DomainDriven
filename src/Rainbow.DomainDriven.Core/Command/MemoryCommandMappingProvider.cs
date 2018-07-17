using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Rainbow.DomainDriven.Domain;
using System.Linq;

namespace Rainbow.DomainDriven.Command
{
    public abstract class MemoryCommandMappingProvider : ICommandMappingProvider
    {

        private ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> _mappings = new ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>>();


        private class MemoryCommandMapping : ICommandMapper
        {
            private MemoryCommandMappingProvider _provider;
            public MemoryCommandMapping(MemoryCommandMappingProvider provider)
            {
                _provider = provider;
            }

            public void Map<TCommand, TAggregateRoot>(Expression<Func<TCommand, Guid>> key)
                where TCommand : ICommand
                where TAggregateRoot : IAggregateRoot
            {
                var type = typeof(TAggregateRoot);
                var mapping = _provider._mappings.GetOrAdd(typeof(TCommand), new ConcurrentDictionary<Delegate, Type>());
                mapping.AddOrUpdate(key.Compile(), type, (a, b) => type);
            }
        }

        public MemoryCommandMappingProvider()
        {
            OnConfiguring(new MemoryCommandMapping(this));
        }

        public abstract void OnConfiguring(ICommandMapper mapper);

        public Dictionary<Guid, Type> Find(ICommand cmd)
        {
            ConcurrentDictionary<Delegate, Type> maps;
            if (!_mappings.TryGetValue(cmd.GetType(), out maps))
                return new Dictionary<Guid, Type>();

            return maps.ToDictionary(p => (Guid)p.Key.DynamicInvoke(cmd), p => p.Value);
        }
    }
}

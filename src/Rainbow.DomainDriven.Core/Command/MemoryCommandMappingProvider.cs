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
        private ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> _mappingLists = new ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>>();

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

            public void MapList<TCommand, TAggregateRoot>(Expression<Func<TCommand, IEnumerable<Guid>>> key)
                where TCommand : ICommand
                where TAggregateRoot : IAggregateRoot
            {
                var type = typeof(TAggregateRoot);
                var mapping = _provider._mappingLists.GetOrAdd(typeof(TCommand), new ConcurrentDictionary<Delegate, Type>());

                var keys = key.Compile();

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
            ConcurrentDictionary<Delegate, Type> mapLists;
            _mappings.TryGetValue(cmd.GetType(), out maps);
            _mappingLists.TryGetValue(cmd.GetType(), out mapLists);

            if (maps == null && mapLists == null) return new Dictionary<Guid, Type>();

            mapLists = mapLists ?? new ConcurrentDictionary<Delegate, Type>();
            maps = maps ?? new ConcurrentDictionary<Delegate, Type>();

            Dictionary<Guid, Type> dicts = new Dictionary<Guid, Type>();
            foreach (var item in mapLists)
            {
                var list = item.Key.DynamicInvoke(cmd) as IEnumerable<Guid>;
                if (list == null) continue;
                foreach (var key in list)
                {
                    dicts.Add(key, item.Value);
                }
            }
            foreach (var item in maps)
            {
                var key = (Guid)item.Key.DynamicInvoke(cmd);
                if (key == Guid.Empty) continue;
                dicts.Add(key, item.Value);
            }

            return dicts;
        }
    }
}

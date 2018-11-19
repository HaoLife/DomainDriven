using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Rainbow.DomainDriven.Domain;
using System.Linq;
using System.Collections;

namespace Rainbow.DomainDriven.Command
{
    public abstract class MemoryCommandMappingProvider : ICommandMappingProvider
    {
        //private ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> _mappings = new ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>>();
        //private ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> _mappingLists = new ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>>();

        private ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> _mappings = new ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>>();

        internal ConcurrentDictionary<Type, ConcurrentDictionary<Delegate, Type>> Mappings => _mappings;


        private MemoryCommandMapping _mapping;

        public MemoryCommandMappingProvider()
        {
            _mapping = new MemoryCommandMapping(this);
            OnConfiguring(_mapping);
        }

        public abstract void OnConfiguring(ICommandMapper mapper);


        private void TryAdd(Dictionary<Guid, Type> dicts, Guid key, Type type)
        {
            if (key == Guid.Empty) return;
            if (dicts.ContainsKey(key)) return;
            dicts.Add(key, type);

        }

        public IEnumerable<KeyValuePair<Guid, Type>> Find(ICommand cmd)
        {
            ConcurrentDictionary<Delegate, Type> maps;
            _mappings.TryGetValue(cmd.GetType(), out maps);
            if (maps == null) return Enumerable.Empty<KeyValuePair<Guid, Type>>();

            Dictionary<Guid, Type> dicts = new Dictionary<Guid, Type>();

            foreach (var item in maps)
            {
                var keyOrKeyList = item.Key.DynamicInvoke(cmd);

                if (keyOrKeyList is IEnumerable<Guid> list)
                {
                    foreach (var key in list)
                    {
                        TryAdd(dicts, key, item.Value);
                    }
                }

                if (keyOrKeyList is Guid g)
                {
                    TryAdd(dicts, g, item.Value);

                }


            }

            return dicts;
        }
    }
}

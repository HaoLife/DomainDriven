using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Command
{

    internal class MemoryCommandMapping : ICommandMapper
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
            var mapping = _provider.Mappings.GetOrAdd(typeof(TCommand), new ConcurrentDictionary<Delegate, Type>());
            mapping.AddOrUpdate(key.Compile(), type, (a, b) => type);
        }
        

        public void MapList<TCommand, TAggregateRoot>(Expression<Func<TCommand, IEnumerable<Guid>>> key)
            where TCommand : ICommand
            where TAggregateRoot : IAggregateRoot
        {
            var type = typeof(TAggregateRoot);
            var mapping = _provider.Mappings.GetOrAdd(typeof(TCommand), new ConcurrentDictionary<Delegate, Type>());

            var keys = key.Compile();

            mapping.AddOrUpdate(key.Compile(), type, (a, b) => type);
        }
        
    }

}

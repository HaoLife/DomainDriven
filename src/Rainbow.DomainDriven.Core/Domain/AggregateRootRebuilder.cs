using Rainbow.DomainDriven.Store;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Rainbow.DomainDriven.Domain
{
    public class AggregateRootRebuilder : IAggregateRootRebuilder
    {
        private ISnapshootStoreFactory _snapshootStoreFactory;
        private IEventStore _eventStore;

        public AggregateRootRebuilder(IEventStore eventStore, ISnapshootStoreFactory snapshootStoreFactory)
        {
            this._eventStore = eventStore;
            this._snapshootStoreFactory = snapshootStoreFactory;

        }
        public TAggregateRoot Rebuild<TAggregateRoot>(Guid id) where TAggregateRoot : IAggregateRoot
        {
            return (TAggregateRoot)Rebuild(typeof(TAggregateRoot), id);
        }

        public IAggregateRoot Rebuild(Type type, Guid id)
        {
            var root = Get(type, new Guid[] { id }).FirstOrDefault();

            Rebuild(root, type, id);

            return root;
        }

        public IAggregateRoot Rebuild(IAggregateRoot aggregateRoot)
        {
            return Rebuild(aggregateRoot.GetType(), aggregateRoot.Id);
        }


        private List<IAggregateRoot> Get(Type rootType, Guid[] ids)
        {
            return _snapshootStoreFactory.Create(rootType).Get(ids);
        }


        public IEnumerable<IAggregateRoot> Rebuild(Type type, Guid[] ids)
        {
            var roots = Get(type, ids);

            List<IAggregateRoot> results = new List<IAggregateRoot>();

            foreach (Guid id in ids)
            {
                var root = Rebuild(roots.FirstOrDefault(a => a.Id == id), type, id);
                if (root != null)
                    results.Add(root);
            }

            return results;
        }


        public IAggregateRoot Rebuild(IAggregateRoot root, Type type, Guid id, int version = 0)
        {
            if (root != null)
            {
                version = root.Version;
            }
            var evs = _eventStore.GetAggregateRootEvents(id, type.Name, version).OrderBy(a => a.Version).ToList();

            if (evs == null || !evs.Any()) return root;

            if (root == null)
            {
                root = Activator.CreateInstance(type, true) as IAggregateRoot;
            }

            evs.ForEach(e => root.ReplayEvent(e));

            return root;
        }
    }
}

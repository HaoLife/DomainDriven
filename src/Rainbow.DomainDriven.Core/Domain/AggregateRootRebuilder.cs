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
        private readonly ConcurrentDictionary<Type, Func<Guid[], List<IAggregateRoot>>> _cache = new ConcurrentDictionary<Type, Func<Guid[], List<IAggregateRoot>>>();

        private static readonly MethodInfo _handleCommandMethod = typeof(AggregateRootRebuilder).GetMethod(nameof(GetAggregateRoot), BindingFlags.Instance | BindingFlags.NonPublic);

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


        public List<IAggregateRoot> Get(Type rootType, Guid[] ids)
        {

            var call = _cache.GetOrAdd(
                key: rootType,
                valueFactory: (type) =>
                {
                    var getHandleMethod = _handleCommandMethod.MakeGenericMethod(type);
                    var parameter = Expression.Parameter(typeof(Guid[]), "ids");
                    var expression =
                        Expression.Lambda<Func<Guid[], List<IAggregateRoot>>>(
                            Expression.Call(null, getHandleMethod, parameter),
                            parameter);
                    return expression.Compile();
                });

            return call(ids);
        }

        private List<IAggregateRoot> GetAggregateRoot<TAggregateRoot>(Guid[] ids) where TAggregateRoot : IAggregateRoot
        {
            return _snapshootStoreFactory.Create<TAggregateRoot>().Get(ids);
        }

        public IEnumerable<IAggregateRoot> Rebuild(Type type, Guid[] ids)
        {
            var roots = Get(type, ids);

            List<IAggregateRoot> results = new List<IAggregateRoot>();

            foreach (Guid id in ids)
            {
                results.Add(Rebuild(roots.FirstOrDefault(a => a.Id == id), type, id));
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

            if (root == null)
            {
                root = Activator.CreateInstance(type, true) as IAggregateRoot;
            }

            evs.ForEach(e => root.ReplayEvent(e));

            return root;
        }
    }
}

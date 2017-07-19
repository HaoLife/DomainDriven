using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MongoDB.Driver;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Repository;
using System.Linq;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoAggregateRootQuery : IAggregateRootQuery
    {

        private readonly IMongoDatabaseProvider _mongoDatabaseProvider;
        private ConcurrentDictionary<Type, Delegate> _cacheListInvokes = new ConcurrentDictionary<Type, Delegate>();
        private ConcurrentDictionary<Type, Delegate> _cacheInvokes = new ConcurrentDictionary<Type, Delegate>();
        public MongoAggregateRootQuery(
            IMongoDatabaseProvider mongoDatabaseProvider)
        {
            this._mongoDatabaseProvider = mongoDatabaseProvider;
        }

        private IMongoCollection<TAggregateRoot> DbSet<TAggregateRoot>(string name)
        {
            return this._mongoDatabaseProvider.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
        }
        private IMongoCollection<TAggregateRoot> DbSet<TAggregateRoot>()
        {
            return this.DbSet<TAggregateRoot>(typeof(TAggregateRoot).Name);
        }

        IEnumerable<TAggregateRoot> IAggregateRootQuery.Get<TAggregateRoot>(params Guid[] keys)
        {
            return this.DbSet<TAggregateRoot>().AsQueryable().Where(a => keys.Contains(a.Id)).ToList();
        }

        TAggregateRoot IAggregateRootQuery.Get<TAggregateRoot>(Guid id)
        {
            return this.DbSet<TAggregateRoot>().Find(a => a.Id == id).FirstOrDefault();
        }

        public IAggregateRoot Get(Type aggregateRootType, Guid id)
        {
            var func = _cacheListInvokes.GetOrAdd(aggregateRootType, GetDelegate);
            var rt = func.DynamicInvoke(id);
            return (IAggregateRoot)rt;

        }

        public IEnumerable<IAggregateRoot> Get(Type aggregateRootType, params Guid[] keys)
        {
            var func = _cacheListInvokes.GetOrAdd(aggregateRootType, GetDelegateList);
            var rt = func.DynamicInvoke(keys);
            List<IAggregateRoot> result = new List<IAggregateRoot>();

            var temps = (IEnumerable)rt;
            foreach (var item in temps)
                result.Add((IAggregateRoot)item);

            return result;
        }

        private Delegate GetDelegateList(Type type)
        {
            var keysType = typeof(Guid[]);
            var keysTypeExp = Expression.Parameter(keysType);
            var instance = Expression.Constant(this);
            var methodType = typeof(IAggregateRootQuery).GetMethod(nameof(Get), new Type[] { keysType })
                .MakeGenericMethod(type);
            var callExp = Expression.Call(instance, methodType, keysTypeExp);
            return Expression.Lambda(callExp, keysTypeExp).Compile();
        }

        private Delegate GetDelegate(Type type)
        {
            var keyType = typeof(Guid);
            var keyTypeExp = Expression.Parameter(keyType);
            var instance = Expression.Constant(this);
            var methodType = typeof(IAggregateRootQuery).GetMethod(nameof(Get), new Type[] { keyType })
                .MakeGenericMethod(type);
            var callExp = Expression.Call(instance, methodType, keyTypeExp);
            return Expression.Lambda(callExp, keyTypeExp).Compile();
        }


    }
}
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Mongo.Framework;
using Rainbow.DomainDriven.Store;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Utilities;
using System.Linq;

namespace Rainbow.DomainDriven.Mongo.Store
{
    public class MongoSnapshootStoreFactory : ISnapshootStoreFactory
    {

        public MongoOptions Options { get; private set; }
        public IMongoDatabase MongoDatabase { get; private set; }
        private IDisposable _optionsReloadToken;
        private ConcurrentDictionary<Type, object> _cache = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, Func<object>> _cacheHandle = new ConcurrentDictionary<Type, Func<object>>();

        private static readonly MethodInfo _handleMethod = typeof(MongoSnapshootStoreFactory).GetMethod(nameof(CreateStore), BindingFlags.Instance | BindingFlags.NonPublic);


        public MongoSnapshootStoreFactory(IOptionsMonitor<MongoOptions> options)
        {
            _optionsReloadToken = options.OnChange(ReloadOptions);
            ReloadOptions(options.CurrentValue);
        }

        private void ReloadOptions(MongoOptions options)
        {
            Options = options;

            var url = new MongoUrl(Options.SnapshootConnection);
            var client = new MongoClient(url);
            var database = client.GetDatabase(options.SnapshootDbName);
            MongoDatabase = database;

            foreach (var item in _cache.Values)
            {
                if (item is IConfigureChange source)
                {
                    source.Reload();
                }
            }
        }


        public ISnapshootStore<TAggregateRoot> CreateOfT<TAggregateRoot>() where TAggregateRoot : IAggregateRoot
        {
            return _cache.GetOrAdd(typeof(TAggregateRoot), new MongoSnapshootStore<TAggregateRoot>(this)) as ISnapshootStore<TAggregateRoot>;
        }

        public ISnapshootStore Create<TAggregateRoot>() where TAggregateRoot : IAggregateRoot
        {

            return _cache.GetOrAdd(typeof(TAggregateRoot), new MongoSnapshootStore<TAggregateRoot>(this)) as ISnapshootStore;
        }


        public ISnapshootStore Create(Type aggregateRootType)
        {
            if (aggregateRootType == typeof(IAggregateRoot)) throw new Exception("聚合根的类型不能为接口");

            var isAggr = aggregateRootType.GetTypeInfo().GetInterfaces().Where(a => a == typeof(IAggregateRoot)).Any();
            if (!isAggr) throw new Exception("类型不是一个聚合根");

            var call = _cacheHandle.GetOrAdd(
                key: aggregateRootType,
                valueFactory: CreateHandle);

            return call() as ISnapshootStore;
        }


        private Func<object> CreateHandle(Type type)
        {
            var getHandleMethod = _handleMethod.MakeGenericMethod(type);
            var instance = Expression.Constant(this);
            var expression =
                Expression.Lambda<Func<object>>(
                    Expression.Call(instance, getHandleMethod));
            return expression.Compile();
        }


        private object CreateStore<TAggregateRoot>() where TAggregateRoot : IAggregateRoot
        {
            return _cache.GetOrAdd(typeof(TAggregateRoot), new MongoSnapshootStore<TAggregateRoot>(this));
        }

    }
}

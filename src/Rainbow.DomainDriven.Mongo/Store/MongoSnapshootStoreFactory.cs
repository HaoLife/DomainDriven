using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Mongo.Framework;
using Rainbow.DomainDriven.Store;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Mongo.Store
{
    public class MongoSnapshootStoreFactory : ISnapshootStoreFactory
    {

        public MongoOptions Options { get; private set; }
        public IMongoDatabase MongoDatabase { get; private set; }
        private IDisposable _optionsReloadToken;
        private System.Collections.Concurrent.ConcurrentDictionary<Type, object> _cache = new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();

        public MongoSnapshootStoreFactory(IOptionsMonitor<MongoOptions> options)
        {
            _optionsReloadToken = options.OnChange(ReloadOptions);
            ReloadOptions(options.CurrentValue);
        }

        private void ReloadOptions(MongoOptions options)
        {
            Options = options;

            var client = new MongoClient(Options.SnapshootConnection);
            var database = client.GetDatabase(Options.SnapshootDbName);
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
    }
}

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
    public class MongoSnapshootQueryFactory : ISnapshootQueryFactory
    {
        public MongoOptions Options { get; private set; }
        public IMongoDatabase MongoDatabase { get; private set; }
        private IDisposable _optionsReloadToken;
        private System.Collections.Concurrent.ConcurrentDictionary<Type, object> _cache = new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();

        public MongoSnapshootQueryFactory(IOptionsMonitor<MongoOptions> options)
        {
            _optionsReloadToken = options.OnChange(ReloadOptions);
            ReloadOptions(options.CurrentValue);
        }

        private void ReloadOptions(MongoOptions options)
        {
            Options = options;


            var url = new MongoUrl(Options.SnapshootConnection);
            var client = new MongoClient(url);
            var database = client.GetDatabase(url.DatabaseName);
            MongoDatabase = database;

            foreach(var item in _cache.Values)
            {
                if(item is IConfigureChange source)
                {
                    source.Reload();
                }
            }
        }


        public ISnapshootQuery<TAggregateRoot> Create<TAggregateRoot>() where TAggregateRoot : IAggregateRoot
        {
            return _cache.GetOrAdd(typeof(TAggregateRoot), new MongoSnapshootQuery<TAggregateRoot>(this)) as ISnapshootQuery<TAggregateRoot>;
        }
    }
}

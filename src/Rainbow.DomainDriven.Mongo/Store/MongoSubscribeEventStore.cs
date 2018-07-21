using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Mongo.Framework;
using Rainbow.DomainDriven.Store;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Rainbow.DomainDriven.Mongo.Store
{
    public class MongoSubscribeEventStore : ISubscribeEventStore
    {
        public MongoOptions Options { get; private set; }
        public IMongoCollection<SubscribeEvent> _collection;
        private IDisposable _optionsReloadToken;
        public MongoSubscribeEventStore(IOptionsMonitor<MongoOptions> options)
        {
            _optionsReloadToken = options.OnChange(ReloadOptions);
            ReloadOptions(options.CurrentValue);
        }

        private void ReloadOptions(MongoOptions options)
        {
            Options = options;

            var client = new MongoClient(Options.SnapshootConnection);
            var database = client.GetDatabase(Options.EventDbName);
            _collection = database.GetCollection<SubscribeEvent>(options.SubscribeEventName, new MongoCollectionSettings() { AssignIdOnInsert = false });

        }

        public SubscribeEvent Get(Guid id)
        {
            return _collection.Find(a => a.Id == id).FirstOrDefault();
        }

        public void Save(SubscribeEvent subscribeEvent)
        {
            _collection.ReplaceOne(a => a.Id == subscribeEvent.Id, subscribeEvent);
        }

        public IEnumerable<SubscribeEvent> Get()
        {
            return _collection.AsQueryable().ToList();
        }
    }
}

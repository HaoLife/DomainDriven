using Microsoft.Extensions.Options;
using MongoDB.Bson;
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
    public class MongoEventStore : IEventStore
    {
        public MongoOptions Options { get; private set; }
        public IMongoCollection<IEvent> _collection;
        private IDisposable _optionsReloadToken;
        public MongoEventStore(IOptionsMonitor<MongoOptions> options)
        {
            _optionsReloadToken = options.OnChange(ReloadOptions);
            ReloadOptions(options.CurrentValue);
        }

        private void ReloadOptions(MongoOptions options)
        {
            Options = options;

            var client = new MongoClient(Options.SnapshootConnection);
            var database = client.GetDatabase(Options.EventDbName);
            _collection = database.GetCollection<IEvent>(options.EventName, new MongoCollectionSettings() { AssignIdOnInsert = false });

        }


        public void AddRange(IEnumerable<IEvent> events)
        {
            _collection.InsertMany(events);
        }

        public IEnumerable<IEvent> GetAggregateRootEvents(Guid aggregateRootId, string aggregateRootTypeName, int version = 0)
        {
            return _collection.Find(a => a.Id == aggregateRootId && a.AggregateRootTypeName == aggregateRootTypeName && a.Version > version).ToList();
        }

        public List<IEvent> Take(int size, long uTCTimestamp = 0)
        {
            var filter = Builders<IEvent>.Filter.Gt(a => a.UTCTimestamp, uTCTimestamp); ;
            return _collection.Find(filter).Limit(size).ToList();
        }
    }
}

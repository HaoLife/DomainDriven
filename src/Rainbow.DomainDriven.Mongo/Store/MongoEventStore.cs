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

            var indk = Builders<IEvent>.IndexKeys;
            var aggrIndex = indk.Combine(
                indk.Ascending(nameof(IEvent.AggregateRootId)),
                indk.Ascending(nameof(IEvent.AggregateRootTypeName)),
                indk.Ascending(nameof(IEvent.Version))
            );


            var timestampIndex = indk.Ascending(nameof(IEvent.UTCTimestamp));


            //Unique = true 这里如果加唯一索引，可能在批量写入事件时出现错误
            var aggrIndexModel = new CreateIndexModel<IEvent>(aggrIndex, new CreateIndexOptions() { Name = "aggr_index" });
            var timestampIndexModel = new CreateIndexModel<IEvent>(timestampIndex, new CreateIndexOptions() { Name = "timestamp_index" });

            _collection.Indexes.CreateOne(timestampIndexModel);
            _collection.Indexes.CreateOne(aggrIndexModel);
        }


        public void AddRange(IEnumerable<IEvent> events)
        {
            _collection.InsertMany(events);
        }

        public IEnumerable<IEvent> GetAggregateRootEvents(Guid aggregateRootId, string aggregateRootTypeName, int version = 0)
        {
            return _collection.Find(a => a.AggregateRootId == aggregateRootId && a.AggregateRootTypeName == aggregateRootTypeName && a.Version > version).ToList();
        }

        public List<IEvent> Take(int size, long utcTimestamp = 0)
        {
            //UTCTimestamp 使用表达式模式出现无法序列化问题
            var filter = Builders<IEvent>.Filter.Gt(nameof(IEvent.UTCTimestamp), utcTimestamp);
            //var filter = Builders<IEvent>.Filter.Empty;
            return _collection.Find(filter).Limit(size).ToList();
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoEventSourceRepository : IEventSourceRepository
    {
        private readonly IMongoCollection<EventSource> _collection;
        public MongoEventSourceRepository(IOptions<MongoOptions> options)
        {
            var client = new MongoClient(options.Value.EventSourceConnectionString);
            var database = client.GetDatabase(options.Value.EventSourceDatabase);
            this._collection = database.GetCollection<EventSource>(nameof(EventSource)); ;
        }
        public void AddRange(IEnumerable<EventSource> events)
        {
            _collection.InsertMany(events);
        }

        public IEnumerable<EventSource> GetAggregateRootEvents(Guid aggregateRootId, string aggregateRootTypeName, int version = 0)
        {
            var filter = Builders<EventSource>.Filter;
            var query = filter.And(
                    filter.Eq(p => p.AggregateRootId, aggregateRootId),
                    filter.Eq(p => p.AggregateRootTypeName, aggregateRootTypeName),
                    filter.Gt(p => p.Event.Version, version)
                );
            return this._collection.Find(query).ToEnumerable();
        }
    }
}
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
        private readonly IMongoDatabaseProvider _mongoDatabaseProvider;
        public MongoEventSourceRepository(IMongoDatabaseProvider mongoDatabaseProvider)
        {
            this._mongoDatabaseProvider = mongoDatabaseProvider;
        }
        public void AddRange(IEnumerable<EventSource> events)
        {
            this._mongoDatabaseProvider.GetEventCollection<EventSource>(typeof(EventSource).Name).InsertMany(events);
        }

        public IEnumerable<EventSource> GetAggregateRootEvents(Guid aggregateRootId, string aggregateRootTypeName, int version = 0)
        {
            var filter = Builders<EventSource>.Filter;
            var query = filter.And(
                    filter.Eq(p => p.AggregateRootId, aggregateRootId),
                    filter.Eq(p => p.AggregateRootTypeName, aggregateRootTypeName),
                    filter.Gt(p => p.Event.Version, version)
                );
            return this._mongoDatabaseProvider.GetEventCollection<EventSource>(typeof(EventSource).Name).Find(query).ToEnumerable();
        }

        public List<EventSource> Take(EventSource eventSource, int size)
        {
            var timestamp = 0L;
            if (eventSource == null)
            {
                timestamp = eventSource.Event.UTCTimestamp;
            }

            var filter = Builders<EventSource>.Filter.Gt("Event.UTCTimestamp", timestamp);
            var esCollection = this._mongoDatabaseProvider.GetEventCollection<EventSource>(typeof(EventSource).Name);
            return esCollection.Find(filter).Limit(size).ToList();
        }
    }
}
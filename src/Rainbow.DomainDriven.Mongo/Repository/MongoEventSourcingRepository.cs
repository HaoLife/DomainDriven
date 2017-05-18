using System;
using System.Collections.Generic;
using MongoDB.Driver;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Repository;
using System.Linq;
using MongoDB.Bson;
using Microsoft.Extensions.Options;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoEventSourcingRepository : IEventSourcingRepository
    {
        private readonly IMongoCollection<DomainEventSourcing> _collection;
        private readonly IMongoDatabase _database;
        private List<string> _collectionNames;
        private static Guid KEY = new Guid("00000000-0000-0000-0000-000000000000");

        public MongoEventSourcingRepository(IOptions<MongoOptions> options)
        {
            var client = new MongoClient(options.Value.EventSourceConnectionString);
            var database = client.GetDatabase(options.Value.EventSourceDatabase);
            this._database = database;
            this._collection = database.GetCollection<DomainEventSourcing>(nameof(DomainEventSourcing));
        }

        EventSource IEventSourcingRepository.Current()
        {
            var result = _collection.Find(a => a.Id == KEY).FirstOrDefault();
            return result == null ? null : result.EventSrouce;
        }

        void IEventSourcingRepository.Save(EventSource current)
        {
            var entity = new DomainEventSourcing()
            {
                Id = new Guid("00000000-0000-0000-0000-000000000000"),
                EventSrouce = current
            };

            var query = Builders<DomainEventSourcing>.Filter.Empty;
            var result = this._collection.ReplaceOne(query, entity, new UpdateOptions() { IsUpsert = true });
        }

        List<EventSource> IEventSourcingRepository.Take(EventSource current, int size)
        {
            var timestamp = 0L;
            if (current != null)
            {
                timestamp = current.Event.UTCTimestamp;
            }

            var filter = Builders<EventSource>.Filter.Gt("Event.UTCTimestamp", timestamp);
            var esCollection = this._database.GetCollection<EventSource>(nameof(EventSource));
            return esCollection.Find(filter).Limit(size).ToList();

        }
    }
}
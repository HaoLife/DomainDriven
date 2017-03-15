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

        DomainEventSource IEventSourcingRepository.Current()
        {
            var result = _collection.Find(a => a.Id == KEY).FirstOrDefault();
            return result == null ? null : result.EventSrouce;
        }

        void IEventSourcingRepository.Save(DomainEventSource current)
        {
            var entity = new DomainEventSourcing()
            {
                Id = new Guid("00000000-0000-0000-0000-000000000000"),
                EventSrouce = current
            };

            var query = Builders<DomainEventSourcing>.Filter.Empty;
            var result = this._collection.ReplaceOne(query, entity, new UpdateOptions() { IsUpsert = true });
        }

        List<DomainEventSource> IEventSourcingRepository.Take(DomainEventSource current, int size)
        {
            if (_collectionNames == null)
            {
                var collections = this._database.ListCollections().ToList();
                _collectionNames = collections
                    .Select(a => a["name"].AsString)
                    .Where(a => a.Contains($"{nameof(DomainEventSource)}"))
                    .ToList();
                _collectionNames.Sort();
            }
            if (!_collectionNames.Any()) return new List<DomainEventSource>();


            var timestamp = 0L;
            List<string> searchCollections = _collectionNames;
            if (current != null)
            {
                timestamp = current.Event.UTCTimestamp;
                var collectionName = $"{nameof(DomainEventSource)}_{new DateTime(timestamp).ToLocalTime().ToString("yyMM")}";
                var index = _collectionNames.FindIndex(a => a.Equals(collectionName));

                searchCollections = _collectionNames.Skip(index + 1).ToList();
            }

            var filter = Builders<DomainEventSource>.Filter.Gt("Event.UTCTimestamp", timestamp);
            List<DomainEventSource> result = new List<DomainEventSource>();
            foreach (var item in searchCollections)
            {
                var esCollection = this._database.GetCollection<DomainEventSource>(item);
                result = esCollection.Find(filter).Limit(size).ToList();
                if (result.Count > 0) break;
            }

            return result;
        }
    }
}
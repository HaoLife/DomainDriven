using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoEventSourceRepository : IEventSourceRepository
    {
        private readonly IMongoDatabase _database;
        public MongoEventSourceRepository(IOptions<MongoOptions> options)
        {
            var client = new MongoClient(options.Value.EventSourceConnectionString);
            var database = client.GetDatabase(options.Value.EventSourceDatabase);
            this._database = database;

        }
        public void AddRange(IEnumerable<DomainEventSource> events)
        {
            var name = $"{nameof(DomainEventSource)}_{DateTime.Now.ToString("yyMM")}";
            var collection = _database.GetCollection<DomainEventSource>(name);
            collection.InsertMany(events);
        }
    }
}
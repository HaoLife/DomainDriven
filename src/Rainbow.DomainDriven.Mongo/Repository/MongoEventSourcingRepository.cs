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
        private readonly IMongoDatabaseProvider _mongoDatabaseProvider;
        private static Guid KEY = new Guid("00000000-0000-0000-0000-000000000000");

        public MongoEventSourcingRepository(IMongoDatabaseProvider mongoDatabaseProvider)
        {
            this._mongoDatabaseProvider = mongoDatabaseProvider;
        }

        EventSource IEventSourcingRepository.Current()
        {
            var collection = this._mongoDatabaseProvider.GetEventCollection<DomainEventSourcing>(typeof(DomainEventSourcing).Name);
            var result = collection.Find(a => a.Id == KEY).FirstOrDefault();
            return result == null ? null : result.EventSrouce;
        }

        void IEventSourcingRepository.Save(EventSource current)
        {
            var entity = new DomainEventSourcing()
            {
                Id = KEY,
                EventSrouce = current
            };

            var query = Builders<DomainEventSourcing>.Filter.Empty;
            var collection = this._mongoDatabaseProvider.GetEventCollection<DomainEventSourcing>(typeof(DomainEventSourcing).Name);
            var result = collection.ReplaceOne(query, entity, new UpdateOptions() { IsUpsert = true });
        }

    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MongoDB.Driver;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Repository;
using System.Linq;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoAggregateRootQuery : IAggregateRootQuery
    {

        private readonly IMongoDatabaseProvider _mongoDatabaseProvider;
        private ConcurrentDictionary<Type, Delegate> _cacheInvokes = new ConcurrentDictionary<Type, Delegate>();
        public MongoAggregateRootQuery(
            IMongoDatabaseProvider mongoDatabaseProvider)
        {
            this._mongoDatabaseProvider = mongoDatabaseProvider;
        }

        private IMongoCollection<TAggregateRoot> DbSet<TAggregateRoot>(string name)
        {
            return this._mongoDatabaseProvider.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
        }
        private IMongoCollection<TAggregateRoot> DbSet<TAggregateRoot>()
        {
            return this.DbSet<TAggregateRoot>(typeof(TAggregateRoot).Name);
        }

        IEnumerable<TAggregateRoot> IAggregateRootQuery.Get<TAggregateRoot>(params Guid[] keys)
        {
            return this.DbSet<TAggregateRoot>().Find(a => keys.Contains(a.Id)).ToList();
        }

        TAggregateRoot IAggregateRootQuery.Get<TAggregateRoot>(Guid id)
        {
            return this.DbSet<TAggregateRoot>().Find(a => a.Id == id).FirstOrDefault();
        }

        public IAggregateRoot Get(Type aggregateRootType, Guid id)
        {
            return this.DbSet<IAggregateRoot>(aggregateRootType.Name).Find(a => a.Id == id).FirstOrDefault();
        }

        public IEnumerable<IAggregateRoot> Get(Type aggregateRootType, params Guid[] keys)
        {
            return this.DbSet<IAggregateRoot>(aggregateRootType.Name).Find(a => keys.Contains(a.Id)).ToList();
        }

    }
}
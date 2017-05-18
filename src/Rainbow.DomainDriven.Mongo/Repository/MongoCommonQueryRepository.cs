using Rainbow.DomainDriven.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.DomainDriven.Domain;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoCommonQueryRepository : IAggregateRootQuery
    {
        private readonly IMongoDatabase _mongoDatabase;
        private ConcurrentDictionary<Type, Delegate> _cacheInvokes = new ConcurrentDictionary<Type, Delegate>();
        public MongoCommonQueryRepository(
            IMongoDatabase mongoDatabase)
        {
            this._mongoDatabase = mongoDatabase;
        }

        private IMongoCollection<TAggregateRoot> DbSet<TAggregateRoot>(string name)
        {
            return this._mongoDatabase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
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

using MongoDB.Driver;
using Rainbow.DomainDriven.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoCommonQueryRepository : IAggregateRootCommonQueryRepository
    {
        private readonly IMongoDatabase _mongoDatabase;
        public MongoCommonQueryRepository(IMongoDatabase mongoDatabase)
        {
            this._mongoDatabase = mongoDatabase;
        }

        private IMongoCollection<TAggregateRoot> Set<TAggregateRoot>()
        {
            return this._mongoDatabase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
        }


        IEnumerable<TAggregateRoot> IAggregateRootCommonQueryRepository.Get<TAggregateRoot>(params Guid[] keys)
        {
            return this.Set<TAggregateRoot>().Find(a => keys.Contains(a.Id)).ToList();
        }

        TAggregateRoot IAggregateRootCommonQueryRepository.Get<TAggregateRoot>(Guid id)
        {
            return this.Set<TAggregateRoot>().Find(p => p.Id == id).FirstOrDefault();
        }
    }
}

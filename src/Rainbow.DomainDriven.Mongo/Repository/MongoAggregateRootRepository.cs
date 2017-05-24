using MongoDB.Driver;
using Rainbow.DomainDriven.Domain;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{

    public class MongoAggregateRootRepository<TAggregateRoot>
        : IAggregateRootRepository<TAggregateRoot>
        where TAggregateRoot : class, IAggregateRoot
    {

        private readonly IMongoCollection<TAggregateRoot> _mongoCollection;

        public MongoAggregateRootRepository(IMongoDatabase mongoDatabase)
        {
            this._mongoCollection = mongoDatabase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
        }

        public void Add(IEnumerable<IAggregateRoot> roots)
        {
            this._mongoCollection.InsertMany(roots.Select(p => p as TAggregateRoot));
        }

        public void Update(IEnumerable<IAggregateRoot> roots)
        {
            List<ReplaceOneModel<TAggregateRoot>> list = new List<ReplaceOneModel<TAggregateRoot>>();
            var filter = Builders<TAggregateRoot>.Filter;
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Eq(p => p.Version, item.Version)
                    );
                list.Add(new ReplaceOneModel<TAggregateRoot>(query, item as TAggregateRoot));
            }
            this._mongoCollection.BulkWrite(list);
        }

        public void Remove(IEnumerable<IAggregateRoot> roots)
        {
            List<DeleteOneModel<TAggregateRoot>> list = new List<DeleteOneModel<TAggregateRoot>>();
            var filter = Builders<TAggregateRoot>.Filter;
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Eq(p => p.Version, item.Version)
                    );
                list.Add(new DeleteOneModel<TAggregateRoot>(query));
            }
            this._mongoCollection.BulkWrite(list);
        }

        public void Add(TAggregateRoot root)
        {
            this._mongoCollection.InsertOne(root);
        }

        public void Update(TAggregateRoot root)
        {
            var filters = Builders<TAggregateRoot>.Filter;
            var query = filters.Eq(p => p.Id, root.Id);
            this._mongoCollection.ReplaceOne(query, root);
        }

        public void Remove(TAggregateRoot root)
        {
            var filters = Builders<TAggregateRoot>.Filter;
            var query = filters.Eq(p => p.Id, root.Id);
            this._mongoCollection.DeleteOne(query);
        }

        TAggregateRoot IAggregateRootRepository<TAggregateRoot>.Get(Guid id)
        {
            return this._mongoCollection.Find(a => a.Id == id).FirstOrDefault();
        }

        IEnumerable<TAggregateRoot> IAggregateRootRepository<TAggregateRoot>.Get(params Guid[] keys)
        {
            return this._mongoCollection.Find(a => keys.Contains(a.Id)).ToList();
        }

        public bool Exsits(Guid id)
        {
            return this._mongoCollection.Find(a => a.Id == id).Any();
        }
    }
}
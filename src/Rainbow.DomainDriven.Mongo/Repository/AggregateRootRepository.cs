using MongoDB.Driver;
using Rainbow.DomainDriven.Domain;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using System.Linq.Expressions;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{

    public class AggregateRootRepository<TAggregateRoot>
        : IAggregateRootRepository
        , IAggregateRootQueryRepository<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {

        private readonly IMongoCollection<TAggregateRoot> _mongoCollection;
        private readonly IQueryable<TAggregateRoot> _mongoCollectionQueryable;
        public AggregateRootRepository(IMongoDatabase mongoDatabase)
        {
            this._mongoCollection = mongoDatabase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
            this._mongoCollectionQueryable = this._mongoCollection.AsQueryable();
        }

        public Type ElementType => _mongoCollectionQueryable.ElementType;

        public Expression Expression => _mongoCollectionQueryable.Expression;

        public IQueryProvider Provider => _mongoCollectionQueryable.Provider;

        public IEnumerator<TAggregateRoot> GetEnumerator() => _mongoCollectionQueryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _mongoCollectionQueryable.GetEnumerator();

        public void Add(IAggregateRoot root)
        {
            this._mongoCollection.InsertOne((TAggregateRoot)root);
        }


        public void Lock(RowLock rowLock, IEnumerable<IAggregateRoot> roots)
        {
            if (!roots.Any()) return;
            var filter = Builders<TAggregateRoot>.Filter;
            var updater = Builders<TAggregateRoot>.Update;
            var rowField = new StringFieldDefinition<TAggregateRoot, string>("Row");
            var expiresField = new StringFieldDefinition<TAggregateRoot, long>("Expires");
            var list = new List<WriteModel<TAggregateRoot>>();
            foreach (var item in roots)
            {
                var query = filter.Or(
                    filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Eq(rowField, string.Empty)
                    ),
                    filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Gte(expiresField, rowLock.Expires)
                    )
                );
                var update = updater.Set(rowField, rowLock.Value).Set(expiresField, rowLock.Expires);
                list.Add(new UpdateOneModel<TAggregateRoot>(query, update));
            }

            var model = this._mongoCollection.BulkWrite(list);
            if (model.Upserts.Count != list.Count) throw new LockException("无法锁定所有对象");
        }

        public void Remove(IAggregateRoot root)
        {
            var builder = Builders<TAggregateRoot>.Filter;
            var filter = builder.Eq(p => p.Id, root.Id);
            this._mongoCollection.DeleteOne(filter);
        }

        public void UnLock(IEnumerable<IAggregateRoot> roots)
        {
            if (!roots.Any()) return;
            var filter = Builders<TAggregateRoot>.Filter;
            var updater = Builders<TAggregateRoot>.Update;
            var rowField = new StringFieldDefinition<TAggregateRoot, string>("Row");
            var expiresField = new StringFieldDefinition<TAggregateRoot, long>("Expires");
            var list = new List<WriteModel<TAggregateRoot>>();
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id)
                    );

                var update = updater.Unset(rowField).Unset(expiresField);
                list.Add(new UpdateOneModel<TAggregateRoot>(query, update));
            }

            var model = this._mongoCollection.BulkWrite(list);
        }

        public void Update(IAggregateRoot root)
        {
            var filter = Builders<TAggregateRoot>.Filter;
            var query = filter.Eq(p => p.Id, root.Id);
            this._mongoCollection.ReplaceOne(query, (TAggregateRoot)root);
        }

    }
}
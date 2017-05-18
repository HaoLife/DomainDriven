using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class AggregateRootQueryRepository<TAggregateRoot> : 
        IAggregateRootQueryable<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
        private readonly IQueryable<TAggregateRoot> _mongoCollectionQueryable;
        public AggregateRootQueryRepository(IMongoDatabase mongoDatabase)
        {
            this._mongoCollectionQueryable = mongoDatabase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name).AsQueryable();
        }

        public Type ElementType => _mongoCollectionQueryable.ElementType;

        public Expression Expression => _mongoCollectionQueryable.Expression;

        public IQueryProvider Provider => _mongoCollectionQueryable.Provider;

        public IEnumerator<TAggregateRoot> GetEnumerator() => _mongoCollectionQueryable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _mongoCollectionQueryable.GetEnumerator();

    }
}
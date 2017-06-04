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
    public class MongoAggregateRootQueryable<TAggregateRoot> :
        IAggregateRootQueryable<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
        private readonly IMongoDatabaseProvider _mongoDatabaseProvider;
        public MongoAggregateRootQueryable(IMongoDatabaseProvider mongoDatabaseProvider)
        {
            this._mongoDatabaseProvider = mongoDatabaseProvider;
        }

        private IQueryable<TAggregateRoot> Queryble => this._mongoDatabaseProvider.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name).AsQueryable();

        public Type ElementType => this.Queryble.ElementType;

        public Expression Expression => this.Queryble.Expression;

        public IQueryProvider Provider => this.Queryble.Provider;

        public IEnumerator<TAggregateRoot> GetEnumerator() => this.Queryble.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.Queryble.GetEnumerator();

    }
}
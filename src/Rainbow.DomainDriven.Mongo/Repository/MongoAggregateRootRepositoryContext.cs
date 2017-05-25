using Rainbow.DomainDriven.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.DomainDriven.Domain;
using MongoDB.Driver;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Mongo.Internal;
using Rainbow.DomainDriven.Cache;
using System.Threading;
using System.Linq.Expressions;
using System.Reflection;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoAggregateRootRepositoryContext :
        IAggregateRootRepositoryContext
    {
        private readonly IAggregateRootOperation _aggregateRootOperation;
        private readonly IMongoDatabase _mongoDatabase;
        private ConcurrentDictionary<Type, Delegate> _cacheInvokes = new ConcurrentDictionary<Type, Delegate>();


        private int _commited;
        private BulkWriteOptions _options = new BulkWriteOptions() { IsOrdered = false };

        public MongoAggregateRootRepositoryContext(
            IMongoDatabase mongoDatabase,
            IAggregateRootOperation aggregateRootOperation)
        {
            this._mongoDatabase = mongoDatabase;
            this._aggregateRootOperation = aggregateRootOperation;
        }

        public void Add(IAggregateRoot aggregate)
        {
            this._aggregateRootOperation.Add(aggregate);
        }

        public void Remove(IAggregateRoot aggregate)
        {
            this._aggregateRootOperation.Remove(aggregate);
        }

        public void Update(IAggregateRoot aggregate)
        {
            this._aggregateRootOperation.Update(aggregate);
        }

        public void Commit()
        {
            try
            {
                List<Exception> errors = new List<Exception>();
                if (Interlocked.Exchange(ref _commited, 1) != 0)
                {
                    throw new InvalidOperationException("重复提交");
                }

                var types = this._aggregateRootOperation.GetAllTypes();
                foreach (var item in types)
                {

                    var func = _cacheInvokes.GetOrAdd(item, GetDelegate);
                    try
                    {
                        func.DynamicInvoke();
                    }
                    catch (TargetInvocationException ex)
                    {
                        errors.Add(ex.InnerException);
                    }

                }
                this._aggregateRootOperation.Clear();
                if (errors.Any()) throw errors.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (Interlocked.Exchange(ref _commited, 0) != 1)
                {
                    throw new InvalidOperationException("无法进行变更状态");
                }
            }
        }

        public void RollBack()
        {
            if (Volatile.Read(ref _commited) != 0)
            {
                throw new InvalidOperationException("已经执行commit 无法进行提交");
            }
            this._aggregateRootOperation.Clear();
        }




        private Delegate GetDelegate(Type type)
        {
            var commandTypeExp = Expression.Parameter(type);
            var instance = Expression.Constant(this);
            var methodType = this.GetType().GetMethod(nameof(Store),
                BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(type);
            var callExp = Expression.Call(instance, methodType);
            return Expression.Lambda(callExp).Compile();
        }

        private void Store<TAggregateRoot>() where TAggregateRoot : class, IAggregateRoot
        {
            var type = typeof(TAggregateRoot);
            var collection = _mongoDatabase.GetCollection<TAggregateRoot>(type.Name);
            List<WriteModel<TAggregateRoot>> list = new List<WriteModel<TAggregateRoot>>();
            BuildAdded(list, this._aggregateRootOperation.GetAdded(type));
            BuildAdded(list, this._aggregateRootOperation.GetUpdated(type));
            BuildAdded(list, this._aggregateRootOperation.GetRemoved(type));
            collection.BulkWrite(list, _options);
        }


        public void BuildAdded<TAggregateRoot>(List<WriteModel<TAggregateRoot>> list, IEnumerable<IAggregateRoot> roots) where TAggregateRoot : class, IAggregateRoot
        {
            var filter = Builders<TAggregateRoot>.Filter;
            foreach (var item in roots)
            {
                list.Add(new InsertOneModel<TAggregateRoot>(item as TAggregateRoot));
            }
        }

        public void BuildUpdated<TAggregateRoot>(List<WriteModel<TAggregateRoot>> list, IEnumerable<IAggregateRoot> roots) where TAggregateRoot : class, IAggregateRoot
        {
            var filter = Builders<TAggregateRoot>.Filter;
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id)
                    );
                list.Add(new ReplaceOneModel<TAggregateRoot>(query, item as TAggregateRoot));
            }
        }

        public void BuildRemoved<TAggregateRoot>(List<WriteModel<TAggregateRoot>> list, IEnumerable<IAggregateRoot> roots) where TAggregateRoot : class, IAggregateRoot
        {
            var filter = Builders<TAggregateRoot>.Filter;
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id)
                    );
                list.Add(new DeleteOneModel<TAggregateRoot>(query));
            }
        }
    }
}

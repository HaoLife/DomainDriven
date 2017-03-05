using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Mongo.Internal
{
    public interface IAggregateRootOperation
    {

        void Add(IAggregateRoot root);
        void Update(IAggregateRoot root);
        void Remove(IAggregateRoot root);
        IAggregateRoot Get(Type type, Guid id);
        Type[] GetAllTypes();

        IEnumerable<IAggregateRoot> GetAdded(Type aggrType);
        IEnumerable<IAggregateRoot> GetUpdated(Type aggrType);
        IEnumerable<IAggregateRoot> GetRemoved(Type aggrType);
        void Clear();
    }
}
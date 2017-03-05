using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Cache
{
    public interface IAggregateRootIndexCache
    {
        void Add<TAggregateRoot>(TAggregateRoot root, string key) where TAggregateRoot : class, IAggregateRoot;

        void Remove<TAggregateRoot>(TAggregateRoot root) where TAggregateRoot : class, IAggregateRoot;

        void Add(IAggregateRoot root, string key);

        void Remove(IAggregateRoot root);
    }
}
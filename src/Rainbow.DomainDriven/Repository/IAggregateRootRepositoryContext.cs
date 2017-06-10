using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootRepositoryContext
    {
        void Add(IAggregateRoot aggregate);
        void Update(IAggregateRoot aggregate);
        void Remove(IAggregateRoot aggregate);
        void Commit();
        void RollBack();
    }
}
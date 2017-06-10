namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootRepositoryContextFactory
    {
         IAggregateRootRepositoryContext Create();
    }
}
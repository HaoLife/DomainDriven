namespace Rainbow.DomainDriven.Command
{
    public interface ICommandMappingProvider
    {
        void OnConfiguring(ICommandMapper mapper);
    }
}
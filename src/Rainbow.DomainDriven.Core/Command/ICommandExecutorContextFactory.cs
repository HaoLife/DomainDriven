namespace Rainbow.DomainDriven.Command
{
    public interface ICommandExecutorContextFactory
    {
         ICommandExecutorContext Create();
    }
}
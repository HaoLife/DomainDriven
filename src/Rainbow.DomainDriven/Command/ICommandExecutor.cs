namespace Rainbow.DomainDriven.Command
{
    public interface ICommandExecutor
    {
         void Handle(ICommand command);
    }
}
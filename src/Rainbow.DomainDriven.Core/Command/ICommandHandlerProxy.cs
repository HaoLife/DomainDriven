namespace Rainbow.DomainDriven.Command
{
    public interface ICommandHandlerProxy
    {
        void Handle(ICommandExecutorContext context, ICommand command);

    }
}
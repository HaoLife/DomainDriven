namespace Rainbow.DomainDriven.Command
{
    public interface ICommandHandler
    {
        void Handle(ICommandContext context, ICommand cmd);
    }
}
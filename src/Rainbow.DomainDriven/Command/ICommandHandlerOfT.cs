namespace Rainbow.DomainDriven.Command
{
    public interface ICommandHandler<in TCommand>
    {
        void Handler(ICommandContext context, TCommand cmd);
    }
}
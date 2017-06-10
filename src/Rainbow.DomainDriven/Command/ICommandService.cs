using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandService
    {
        void Publish(ICommand command, MessageDescribe describe);
    }
}
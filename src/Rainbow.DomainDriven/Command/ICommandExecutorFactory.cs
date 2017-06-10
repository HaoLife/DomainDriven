using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandExecutorFactory
    {
        ICommandExecutor Create(MessageDescribe describe);
    }
}
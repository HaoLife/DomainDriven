using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandExecutorProxy
    {
         void Handle(DomainMessage domainMessage);
    }
}
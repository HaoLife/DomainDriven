using System;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandExecutorProxyProvider
    {
         ICommandExecutorProxy GetCommandExecutorProxy(Type commandType);
    }
}
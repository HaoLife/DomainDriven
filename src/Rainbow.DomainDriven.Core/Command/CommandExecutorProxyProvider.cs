using System;

namespace Rainbow.DomainDriven.Command
{
    public class CommandExecutorProxyProvider : ICommandExecutorProxyProvider
    {

        private readonly IServiceProvider _serviceProvider;

        public CommandExecutorProxyProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public ICommandExecutorProxy GetCommandExecutorProxy(Type commandType)
        {
            var genericType = typeof(CommandExecutorProxy<>).MakeGenericType(commandType);
            var proxy = this._serviceProvider.GetService(genericType);
            if (proxy != null) return proxy as ICommandExecutorProxy;

            throw new NotImplementedException($"没有找到类型：{commandType.Name}");
        }


    }
}
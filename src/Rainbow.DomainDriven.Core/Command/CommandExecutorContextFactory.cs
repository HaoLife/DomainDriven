using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rainbow.DomainDriven.Command
{
    public class CommandExecutorContextFactory : ICommandExecutorContextFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public CommandExecutorContextFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public ICommandExecutorContext Create()
        {
            return this._serviceProvider.GetService<ICommandExecutorContext>();
        }
    }
}
using System;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Queue;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class CommandExecutorHandler : IQueueHandler<DomainMessage>
    {
        private readonly ICommandExecutorProxyProvider _commandExecutorProxyProvider;
        private readonly ILogger<CommandExecutorHandler> _logger;
        public CommandExecutorHandler(
            ICommandExecutorProxyProvider commandExecutorProxyProvider
            , ILogger<CommandExecutorHandler> logger
            )
        {
            this._commandExecutorProxyProvider = commandExecutorProxyProvider;
            this._logger = logger;
        }

        public void Handle(DomainMessage message, long sequence, bool isEnd)
        {
            try
            {
                var executor = _commandExecutorProxyProvider.GetCommandExecutorProxy(message.Content.GetType());
                executor.Handle(message);
            }
            catch (DomainException dex)
            {
                this._logger.LogInformation(dex.Message);
            }
            catch (Exception ex)
            {
                this._logger.LogError(LogEvent.Frame, ex, $"execute name:{nameof(CommandExecutorHandler)} error");
            }
        }

    }
}
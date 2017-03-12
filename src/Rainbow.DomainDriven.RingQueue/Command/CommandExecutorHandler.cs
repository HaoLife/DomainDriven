using System;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.DomainDriven.RingQueue.Queue;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class CommandExecutorHandler : IQueueHandler<DomainMessage>
    {
        private readonly ICommandExecutorProxyProvider _commandExecutorProxyProvider;
        private readonly ILogger<CommandExecutorHandler> _logger;
        private readonly IMessageListening _messageListening;
        public CommandExecutorHandler(
            ICommandExecutorProxyProvider commandExecutorProxyProvider
            , IMessageListening messageListening
            , ILogger<CommandExecutorHandler> logger
            )
        {
            this._commandExecutorProxyProvider = commandExecutorProxyProvider;
            this._messageListening = messageListening;
            this._logger = logger;
        }

        private void Notice(DomainMessage message, Exception ex)
        {
            if (!string.IsNullOrEmpty(message.Head.ReplyKey))
            {
                var noticeMessage = new NoticeMessage() { IsSuccess = false, Exception = ex };
                this._messageListening.Notice(message.Head.ReplyKey, noticeMessage);
            }
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
                this.Notice(message, dex);
            }
            catch (Exception ex)
            {
                this._logger.LogError(LogEvent.Frame, ex, $"execute name:{nameof(CommandExecutorHandler)} error");
                this.Notice(message, ex);
            }
        }

    }
}
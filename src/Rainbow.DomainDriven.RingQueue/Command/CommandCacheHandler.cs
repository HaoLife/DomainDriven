using System;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Queue;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class CommandCacheHandler : IQueueHandler<DomainMessage>
    {
        private readonly ILogger<CommandCacheHandler> _logger;
        public CommandCacheHandler(
            ILogger<CommandCacheHandler> logger
        )
        {
            this._logger = logger;
        }
        public void Handle(DomainMessage message, long sequence, bool isEnd)
        {
            //throw new NotImplementedException();
        }
    }
}
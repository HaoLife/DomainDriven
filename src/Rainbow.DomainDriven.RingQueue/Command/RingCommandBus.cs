using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.RingQueue.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.MessageQueue.Ring;
using Rainbow.DomainDriven.Domain;
using Microsoft.Extensions.Logging;
using System.Linq;
using Rainbow.DomainDriven.Store;
using Rainbow.DomainDriven.Event;
using System.Threading;
using Rainbow.DomainDriven.RingQueue.Event;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingCommandBus : ICommandBus
    {
        private IRingBufferProcess _ringBufferProcess;
        private ILogger _logger;


        private IEventHandleSubject _eventHandleSubject;
        private IEventHandleObserver _snapshootEventHandleObserver;


        public RingCommandBus(
            IRingBufferProcess ringBufferProcess
            , IEventHandleSubject eventHandleSubject
            , ILoggerFactory loggerFactory)
        {
            _ringBufferProcess = ringBufferProcess;
            _eventHandleSubject = eventHandleSubject;
            _logger = loggerFactory.CreateLogger<RingCommandBus>();
            InitializeSubject();
        }


        private void InitializeSubject()
        {
            _eventHandleSubject.Remove(Constant.SnapshootSubscribeId);
            _snapshootEventHandleObserver = new EventHandleObserver(Constant.SnapshootSubscribeId);
            _eventHandleSubject.Add(_snapshootEventHandleObserver);
        }


        public Task Publish(ICommand command)
        {
            if (!_ringBufferProcess.IsStart)
            {
                throw new Exception("队列执行程序未启动，无法发送命令到命令执行队列中");
            }

            var queue = _ringBufferProcess.GetCommand();

            var msg = new CommandMessage(command);
            var index = queue.Next();
            queue[index].Value = msg;
            queue.Publish(index);
            _logger.LogDebug($"开始发送事件:{index} - {command.Id}");

            return Task.Factory.StartNew(() => HandleWait(command, msg, index));
        }

        private void HandleWait(ICommand command, CommandMessage msg, long index)
        {
            if (command.Wait == WaitLevel.NotWait) return;

            msg.Notice.WaitOne();
            _logger.LogDebug($"完成事件处理:{index} - {command.Id}");
            ReplyMessage message = msg.Reply;
            if (message.CommandId == command.Id)
            {
                if (!message.IsSuccess) throw message.Exception;
            }

            if (command.Wait == WaitLevel.Handle) return;

            var timestamp = _snapshootEventHandleObserver.SubscribeEvent?.UTCTimestamp ?? 0;
            while (timestamp < message.LastEventUTCTimestamp)
            {
                Thread.Sleep(20);
                timestamp = _snapshootEventHandleObserver.SubscribeEvent?.UTCTimestamp ?? 0;
            }
            _logger.LogDebug($"完成事件快照:{index} - {command.Id}");

        }
    }
}

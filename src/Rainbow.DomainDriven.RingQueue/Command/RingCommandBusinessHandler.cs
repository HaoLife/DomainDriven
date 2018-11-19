using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Event;
using Rainbow.MessageQueue.Ring;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using Rainbow.DomainDriven.Store;
using Rainbow.DomainDriven.Domain;
using Microsoft.Extensions.Logging;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingCommandBusinessHandler : AbstractBatchMessageHandler<CommandMessage>
    {

        private readonly ConcurrentDictionary<Type, Action<RingCommandContext, ICommand>> _cache = new ConcurrentDictionary<Type, Action<RingCommandContext, ICommand>>();
        private static readonly MethodInfo _handleMethod = typeof(RingCommandBusinessHandler).GetMethod(nameof(HandleCommand), BindingFlags.Instance | BindingFlags.NonPublic);

        private RingCommandContext context;
        private IContextCache _contextCache;
        private ICommandHandlerFactory _commandHandlerFactory;
        private IEventStore _eventStore;
        private IEventBus _eventBus;

        private ICommandRegister _commandRegister;
        private IAggregateRootRebuilder _aggregateRootRebuilder;
        private ILogger _logger;


        public RingCommandBusinessHandler(
            IContextCache contextCache
            , ICommandHandlerFactory commandHandlerFactory
            , IEventStore eventStore
            , IEventBus eventBus
            , ICommandRegister commandRegister
            , IAggregateRootRebuilder aggregateRootRebuilder
            , ILoggerFactory loggerFactory)
            : base(1000)
        {
            this.context = new RingCommandContext(contextCache, aggregateRootRebuilder);
            this._contextCache = contextCache;
            this._commandHandlerFactory = commandHandlerFactory;
            this._eventStore = eventStore;
            this._eventBus = eventBus;
            this._commandRegister = commandRegister;
            this._aggregateRootRebuilder = aggregateRootRebuilder;
            this._logger = loggerFactory.CreateLogger<RingCommandBusinessHandler>();


        }

        public override void Handle(CommandMessage[] messages, long endSequence)
        {
            List<IEvent> evs = new List<IEvent>();
            //回复消息
            ReplyMessage[] replys = new ReplyMessage[messages.Length];
            var seq = endSequence - messages.Length + 1;
            CommandBusinessContext businessContext = new CommandBusinessContext();
            for (int i = 0; i < messages.Length; i++)
            {
                businessContext.Clear();

                HandleMessage(businessContext, messages[i]);

                replys[i] = businessContext.Reply;
                if (businessContext.UncommittedEvents != null && businessContext.UncommittedEvents.Any())
                {
                    evs.AddRange(businessContext.UncommittedEvents);
                }
            }
            //存储事件
            //发送消息到消息总线
            //通知回复消息

            SaveEvent(evs, replys);
            SendEvent(evs, replys);

            NoticeEvent(messages, replys);




        }

        private void SaveEvent(List<IEvent> evs, ReplyMessage[] replys)
        {
            if (evs == null || !evs.Any()) return;


            try
            {
                _eventStore.AddRange(evs);
            }
            catch (Exception ex)
            {
                foreach (var item in replys)
                {
                    item.Exception = ex;
                    item.IsSuccess = false;
                }

                _logger.LogError(ex, $"存储事件失败，错误消息：{ex.Message}");
            }


        }

        private void SendEvent(List<IEvent> evs, ReplyMessage[] replys)
        {

            if (evs == null || !evs.Any()) return;
            try
            {
                _eventBus.Publish(evs.ToArray());

            }
            catch (Exception ex)
            {
                foreach (var item in replys)
                {
                    item.Exception = ex;
                    item.IsSuccess = false;
                }
                _logger.LogError(ex, $"推送事件失败，错误消息：{ex.Message}");
            }
        }

        private void NoticeEvent(CommandMessage[] messages, ReplyMessage[] replys)
        {

            try
            {
                for (int i = 0; i < messages.Length; i++)
                {
                    messages[i].Reply = replys[i];
                    messages[i].Notice.Set();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"事件通知失败 错误消息：{ex.Message}");
            }



        }

        private void HandleMessage(CommandBusinessContext businessContext, CommandMessage commandMessage)
        {

            var message = commandMessage.Cmd;
            try
            {
                var call = _cache.GetOrAdd(
                    key: message.GetType(),
                    valueFactory: CreateHandleCommand);

                call(context, message);

                var unEvts = context.TrackedAggregateRoots.SelectMany(p => p.UncommittedEvents);
                businessContext.UncommittedEvents = unEvts;
                businessContext.Reply = new ReplyMessage(message.Id) { LastEventUTCTimestamp = unEvts?.Select(a => a.UTCTimestamp).LastOrDefault() ?? 0 };


            }
            catch (Exception ex)
            {
                //加入错误回复消息
                //清空上下文，移除缓存
                businessContext.Reply = new ReplyMessage(message.Id, ex);

                //如果聚合根中包含有创建的事件，则该聚合根可直接删除。
                var emptyRoots = context.TrackedAggregateRoots.Where(a => a.UncommittedEvents.Count() == 0);
                var clearRoots = context.TrackedAggregateRoots.Except(emptyRoots);

                foreach (var root in clearRoots)
                {
                    _contextCache.Remove(root);
                }
            }
            finally
            {
                context.Clear();
            }
        }

        private Action<RingCommandContext, ICommand> CreateHandleCommand(Type type)
        {
            var getHandleMethod = _handleMethod.MakeGenericMethod(type);
            var instance = Expression.Constant(this);
            var parameter = Expression.Parameter(typeof(RingCommandContext), "context");
            var parameter2 = Expression.Parameter(typeof(ICommand), "command");
            var expression =
                Expression.Lambda<Action<RingCommandContext, ICommand>>(
                    Expression.Call(instance, getHandleMethod, parameter, parameter2),
                    parameter, parameter2);
            return expression.Compile();
        }


        private void HandleCommand<TCommand>(RingCommandContext context, ICommand command) where TCommand : ICommand
        {
            var handlerType = _commandRegister.FindHandlerType<TCommand>();
            var handler = _commandHandlerFactory.Create<TCommand>(handlerType);

            handler.Handle(context, (TCommand)command);
        }

    }
}

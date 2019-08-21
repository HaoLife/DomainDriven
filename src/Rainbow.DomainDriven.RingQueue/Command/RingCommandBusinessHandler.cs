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
            CommandBusinessContext[] businessContexts = new CommandBusinessContext[messages.Length];

            for (int i = 0; i < messages.Length; i++)
            {
                CommandBusinessContext businessContext = new CommandBusinessContext();
                businessContext.Message = messages[i];
                businessContexts[i] = businessContext;

                HandleMessage(businessContext);
            }
            //存储事件
            //发送消息到消息总线
            //通知回复消息

            SaveEvent(businessContexts);
            SendEvent(businessContexts);

            NoticeEvent(businessContexts);




        }

        private void SaveEvent(CommandBusinessContext[] contexts)
        {
            var evs = contexts.SelectMany(a => a.UncommittedEvents ?? Enumerable.Empty<IEvent>()).ToList();
            if (evs == null || !evs.Any()) return;


            try
            {
                _eventStore.AddRange(evs);
            }
            catch (Exception ex)
            {

                foreach (var item in contexts)
                {
                    item.Reply.Exception = ex;
                    item.Reply.IsSuccess = false;
                }

                _logger.LogError(ex, $"存储事件失败，错误消息：{ex.Message}");
            }


        }

        private void SendEvent(CommandBusinessContext[] contexts)
        {
            var evs = contexts.SelectMany(a => a.UncommittedEvents ?? Enumerable.Empty<IEvent>()).ToList();
            if (evs == null || !evs.Any()) return;

            try
            {
                _eventBus.Publish(evs.ToArray());

            }
            catch (Exception ex)
            {
                foreach (var item in contexts)
                {
                    item.Reply.Exception = ex;
                    item.Reply.IsSuccess = false;
                }
                _logger.LogError(ex, $"推送事件失败，错误消息：{ex.Message}");
            }
        }

        private void NoticeEvent(CommandBusinessContext[] contexts)
        {

            try
            {
                foreach (var item in contexts)
                {
                    item.Message.Reply = item.Reply;
                    item.Message.Notice.Set();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"事件通知失败 错误消息：{ex.Message}");
            }



        }

        private void HandleMessage(CommandBusinessContext businessContext)
        {

            var message = businessContext.Message.Cmd;
            try
            {
                var call = _cache.GetOrAdd(
                    key: message.GetType(),
                    valueFactory: CreateHandleCommand);

                call(context, message);

                var unEvts = context.TrackedAggregateRoots.SelectMany(p => p.UncommittedEvents);
                businessContext.UncommittedEvents = unEvts.ToList();
                businessContext.Reply = new ReplyMessage(message.Id) { LastEventUTCTimestamp = unEvts?.Select(a => a.UTCTimestamp).LastOrDefault() ?? 0 };

                foreach (var root in context.TrackedAggregateRoots)
                {
                    _contextCache.Set(root);
                }
            }
            catch (Exception ex)
            {
                //加入错误回复消息
                //清空上下文，移除缓存
                businessContext.Reply = new ReplyMessage(message.Id, ex);

                //上下文的聚合根中有事件的则说明被执行过，需要删除进行重建
                var clearRoots = context.TrackedAggregateRoots.Where(a => a.UncommittedEvents.Any());

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

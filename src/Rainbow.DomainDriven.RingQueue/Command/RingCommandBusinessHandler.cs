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

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingCommandBusinessHandler : IMessageHandler<ICommand>
    {

        private readonly ConcurrentDictionary<Type, Action<RingCommandContext, ICommand>> _cache = new ConcurrentDictionary<Type, Action<RingCommandContext, ICommand>>();
        private static readonly MethodInfo _handleCommandMethod = typeof(RingCommandBusinessHandler).GetMethod(nameof(HandleCommand), BindingFlags.Instance | BindingFlags.NonPublic);

        private RingCommandContext context;
        private IContextCache _contextCache;
        private ICommandHandlerFactory _commandHandlerFactory;
        private IEventStore _eventStore;
        private IEventBus _eventBus;
        private IReplyBus _replyBus;

        private ICommandRegister _commandRegister;
        private IAggregateRootRebuilder _aggregateRootRebuilder;


        public RingCommandBusinessHandler(
            IContextCache contextCache
            , ICommandHandlerFactory commandHandlerFactory
            , IEventStore eventStore
            , IEventBus eventBus
            , IReplyBus replyBus
            , ICommandRegister commandRegister
            , IAggregateRootRebuilder aggregateRootRebuilder)
        {
            this.context = new RingCommandContext(contextCache, aggregateRootRebuilder);
            this._contextCache = contextCache;
            this._commandHandlerFactory = commandHandlerFactory;
            this._eventStore = eventStore;
            this._eventBus = eventBus;
            this._replyBus = replyBus;
            this._commandRegister = commandRegister;
            this._aggregateRootRebuilder = aggregateRootRebuilder;


        }

        public void Handle(ICommand[] messages)
        {
            List<IEvent> evs = new List<IEvent>();
            //回复消息
            List<ReplyMessage> replys = new List<ReplyMessage>();

            foreach (var message in messages)
            {
                try
                {
                    var call = _cache.GetOrAdd(
                        key: message.GetType(),
                        valueFactory: (type) =>
                        {
                            var getHandleMethod = _handleCommandMethod.MakeGenericMethod(type);
                            var parameter = Expression.Parameter(typeof(RingCommandContext), "context");
                            var parameter2 = Expression.Parameter(typeof(ICommand), "command");
                            var expression =
                                Expression.Lambda<Action<RingCommandContext, ICommand>>(
                                    Expression.Call(null, getHandleMethod, parameter, parameter2),
                                    parameter, parameter2);
                            return expression.Compile();
                        });

                    call(context, message);

                    var unEvts = context.TrackedAggregateRoots.SelectMany(p => p.UncommittedEvents);
                    evs.AddRange(unEvts);
                    //加入回复消息
                    replys.Add(new ReplyMessage(message.Id));

                }
                catch (Exception ex)
                {
                    //加入错误回复消息
                    //清空上下文，移除缓存
                    replys.Add(new ReplyMessage(message.Id, ex));
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
            //存储事件
            //发送消息到消息总线
            //发送回复消息
            _eventStore.AddRange(evs);
            _eventBus.Publish(evs.ToArray());
            _replyBus.Publish(replys.ToArray());


        }


        private void HandleCommand<TCommand>(RingCommandContext context, ICommand command) where TCommand : ICommand
        {
            var handlerType = _commandRegister.FindHandlerType<TCommand>();
            var handler = _commandHandlerFactory.Create<TCommand>(handlerType);

            handler.Handle(context, (TCommand)command);
        }
    }
}

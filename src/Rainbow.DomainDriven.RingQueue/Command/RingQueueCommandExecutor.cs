using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;
using System.Linq;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Message;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    //todo:modify by haozi 未实现
    //1.通知
    public class RingQueueCommandExecutor : ICommandExecutor
    {
        private readonly ICommandHandlerSelector _commandExecutorSelector;
        private readonly ICommandHandlerActivator _commandHandlerActivator;
        private readonly IEventService _eventService;
        private readonly ICommandExecutorContextFactory _commandExecutorContextFactory;
        private readonly ICommandExecutorContext _commandExecutorContext;
        private readonly IRollbackService _rollbackService;
        private readonly IAggregateRootCache _aggregateRootCache;
        private readonly IAggregateRootIndexCache _aggregateRootIndexCache;

        public RingQueueCommandExecutor(
             ICommandHandlerSelector commandHandlerSelector
            , ICommandHandlerActivator commandHandlerActivator
            , IEventService eventService
            , ICommandExecutorContextFactory commandExecutorContextFactory
            , IRollbackService rollbackService
            , IAggregateRootCache aggregateRootCache
            , IAggregateRootIndexCache aggregateRootIndexCache
            )
        {
            this._commandExecutorSelector = commandHandlerSelector;
            this._commandHandlerActivator = commandHandlerActivator;
            this._eventService = eventService;
            this._commandExecutorContextFactory = commandExecutorContextFactory;
            this._commandExecutorContext = commandExecutorContextFactory.Create();
            this._rollbackService = rollbackService;
            this._aggregateRootCache = aggregateRootCache;
            this._aggregateRootIndexCache = aggregateRootIndexCache;
        }
        public void Handle<TCommand>(DomainMessage cmd) where TCommand : class
        {

            var executorType = this._commandExecutorSelector.FindExecutorType<TCommand>();

            if (executorType == null) throw new NullReferenceException($"没有找到 {typeof(TCommand).Name} 的执行器类型");

            var executor = this._commandHandlerActivator.Create<TCommand>(executorType);
            if (executor == null) throw new NullReferenceException($"没有找到 {typeof(TCommand).Name} 的执行器类型");

            try
            {
                executor.Handler(_commandExecutorContext, cmd.Content as TCommand);
                var message = BuildEventMessage(cmd.Head, _commandExecutorContext.TrackedAggregateRoots);
                _eventService.Publish(message);
            }
            catch (Exception ex)
            {
                if (_commandExecutorContext.TrackedAggregateRoots.Any())
                {
                    var redoRoots = _rollbackService.Redo(_commandExecutorContext.TrackedAggregateRoots);
                    foreach (var root in redoRoots)
                    {
                        _aggregateRootCache.Set(root);
                    }
                }
                throw ex;
            }
            finally
            {
                _commandExecutorContext.Clear();
            }
        }

        private DomainMessage BuildEventMessage(MessageHead head, IEnumerable<IAggregateRoot> roots)
        {
            var domainSources = roots
                .SelectMany(p => p.UncommittedEvents.Select(a => new DomainEventSource(a, p.GetType().Name, p.Id)));
            var stream = new DomainEventStream() { EventSources = domainSources.ToList() };

            MessageHead eventHead = null;
            if (!IsNotice(head))
            {
                eventHead = new MessageHead(Guid.NewGuid().ToShort(), head.ReplyKey, head.Priority, head.Consistency);
            }
            else
            {
                eventHead = new MessageHead(Guid.NewGuid().ToShort(), head.Priority, head.Consistency);
            }
            return new DomainMessage(eventHead, stream);
        }

        protected bool IsNotice(MessageHead head)
        {
            return head.Consistency == ConsistencyLevel.Lose;
        }


    }
}
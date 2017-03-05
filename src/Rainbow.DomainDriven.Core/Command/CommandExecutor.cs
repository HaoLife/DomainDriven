using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Cache;

namespace Rainbow.DomainDriven.Command
{
    public class CommandExecutor :
        ICommandExecutor
    {
        private readonly ICommandHandlerSelector _commandExecutorSelector;
        private readonly ICommandHandlerActivator _commandHandlerActivator;
        private readonly IAggregateRootRepositoryContext _repositoryContext;
        private readonly IEventService _eventService;
        private readonly ICommandExecutorContextFactory _commandExecutorContextFactory;

        public CommandExecutor(
            ICommandHandlerSelector commandHandlerSelector,
            ICommandHandlerActivator commandHandlerActivator,
            IAggregateRootRepositoryContext repositoryContext,
            IEventService eventService,
            ICommandExecutorContextFactory commandExecutorContextFactory
            )
        {
            this._commandExecutorSelector = commandHandlerSelector;
            this._commandHandlerActivator = commandHandlerActivator;
            this._repositoryContext = repositoryContext;
            this._eventService = eventService;
            this._commandExecutorContextFactory = commandExecutorContextFactory;
        }
        public void Handle<TCommand>(DomainMessage cmd) where TCommand : class
        {

            ICommandExecutorContext context = this._commandExecutorContextFactory.Create();

            var executorType = this._commandExecutorSelector.FindExecutorType<TCommand>();

            if (executorType == null) throw new NullReferenceException($"没有找到 {typeof(TCommand).Name} 的执行器类型");

            var executor = this._commandHandlerActivator.Create<TCommand>(executorType);
            if (executor == null) throw new NullReferenceException($"没有找到 {typeof(TCommand).Name} 的执行器类型");

            try
            {
                executor.Handler(context, cmd.Content as TCommand);
                this.HandleContext(context.TrackedAggregateRoots);
                _repositoryContext.Commit();
                var message = BuildEventMessage(cmd.Head, context.TrackedAggregateRoots);
                _eventService.Publish(message);
            }
            catch (Exception ex)
            {
                _repositoryContext.RollBack();
                throw ex;
            }
            finally
            {
                context.Clear();
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
            return head.Consistency == ConsistencyLevel.Lose || head.Consistency == ConsistencyLevel.Finally;
        }

        protected void HandleContext(IEnumerable<IAggregateRoot> roots)
        {
            foreach (var item in roots)
            {
                var isAdd = item.UncommittedEvents.Any(a => a.Version == 1);
                if (isAdd)
                {
                    this._repositoryContext.Add(item);
                }
                else
                {
                    this._repositoryContext.Update(item);
                }
            }

        }
    }
}

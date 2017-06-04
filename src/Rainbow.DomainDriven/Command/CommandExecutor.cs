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
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ICommandHandlerProxy _commandHandlerProxy;
        private readonly IAggregateRootRepositoryContext _aggregateRootRepositoryContext;
        private readonly ICommandExecutorContextFactory _commandExecutorContextFactory;
        private readonly IEventExecutor _eventExecutor;

        public CommandExecutor(
             ICommandHandlerProxy commandHandlerProxy,
             IAggregateRootRepositoryContext aggregateRootRepositoryContext,
             ICommandExecutorContextFactory commandExecutorContextFactory,
             IEventExecutor eventExecutor
            )
        {
            this._commandHandlerProxy = commandHandlerProxy;
            this._aggregateRootRepositoryContext = aggregateRootRepositoryContext;
            this._commandExecutorContextFactory = commandExecutorContextFactory;
            this._eventExecutor = eventExecutor;
        }

        protected virtual void Store(IEnumerable<IAggregateRoot> roots)
        {
            foreach (var item in roots)
            {
                var isAdd = item.UncommittedEvents.Any(a => a.Version == 1);
                if (isAdd)
                {
                    this._aggregateRootRepositoryContext.Add(item);
                }
                else
                {
                    this._aggregateRootRepositoryContext.Update(item);
                }
            }
            this._aggregateRootRepositoryContext.Commit();
        }


        protected virtual DomainMessage<EventStream> BuildEventMessage(MessageHead head, IEnumerable<IAggregateRoot> roots)
        {
            var events = roots
                .SelectMany(p => p.UncommittedEvents.Select(item => new EventSource(item, p.GetType().Name, p.Id)));

            var stream = new EventStream(events.ToList());
            var eventHead = new MessageHead(Priority.Normal, head.Consistency);
            return new DomainMessage<EventStream>(eventHead, stream);
        }


        public void Handle(params DomainMessage<ICommand>[] messages)
        {
            ICommandExecutorContext context = this._commandExecutorContextFactory.Create();

            foreach (var message in messages)
            {
                try
                {
                    this._commandHandlerProxy.Handle(context, message.Content);
                    this.Store(context.TrackedAggregateRoots);
                    var evtMessage = BuildEventMessage(message.Head, context.TrackedAggregateRoots);
                    Task.Factory.StartNew(item => this._eventExecutor.Handle(item as DomainMessage<EventStream>), evtMessage);

                }
                catch (Exception ex)
                {
                    _aggregateRootRepositoryContext.RollBack();
                    throw ex;
                }
                finally
                {
                    context.Clear();
                }
            }

        }
    }
}

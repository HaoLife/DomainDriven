using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Repository;
using System.Linq;

namespace Rainbow.DomainDriven.Command
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ICommandExecutorContextFactory _commandExecutorContextFactory;
        private readonly ICommandHandler _commandHandler;
        private readonly IAggregateRootRepositoryContextFactory _aggregateRootRepositoryContextFactory;
        private readonly IEventExecutor _eventExecutor;
        public CommandExecutor(
             ICommandHandler commandHandler,
             IAggregateRootRepositoryContextFactory aggregateRootRepositoryContextFactory,
             ICommandExecutorContextFactory commandExecutorContextFactory,
             IEventExecutor eventExecutor
            )
        {
            this._commandHandler = commandHandler;
            this._aggregateRootRepositoryContextFactory = aggregateRootRepositoryContextFactory;
            this._commandExecutorContextFactory = commandExecutorContextFactory;
            this._eventExecutor = eventExecutor;

        }
        public void Handle(ICommand command)
        {
            var context = _commandExecutorContextFactory.Create();

            try
            {
                this._commandHandler.Handle(context, command);
                this.Store(context.TrackedAggregateRoots);
                var sources = BuildEventSources(context.TrackedAggregateRoots);
                Task.Factory.StartNew(item => this._eventExecutor.Handle(item as EventSource[]), sources);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                context.Clear();
            }
        }

        protected virtual void Store(IEnumerable<IAggregateRoot> roots)
        {
            var context = _aggregateRootRepositoryContextFactory.Create();

            try
            {
                foreach (var item in roots)
                {
                    var isAdd = item.UncommittedEvents.Any(a => a.Version == 1);
                    if (isAdd)
                    {
                        context.Add(item);
                    }
                    else
                    {
                        context.Update(item);
                    }
                }
                context.Commit();
            }
            catch (Exception ex)
            {
                context.RollBack();
                throw ex;
            }
        }

        protected virtual EventSource[] BuildEventSources(IEnumerable<IAggregateRoot> roots)
        {
            var events = roots
                .SelectMany(p => p.UncommittedEvents.Select(item => new EventSource(item, p.GetType().Name, p.Id)));

            return events.ToArray();
        }

    }
}
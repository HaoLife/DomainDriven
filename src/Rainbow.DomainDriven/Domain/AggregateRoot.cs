using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Domain
{
    public abstract class AggregateRoot : IAggregateRoot
    {
        private readonly Queue<IEvent> _uncommittedEvents = new Queue<IEvent>();

        public virtual Guid Id { get; protected set; }

        public IEnumerable<IEvent> UncommittedEvents => _uncommittedEvents.ToArray();

        public int Version { get; protected set; }

        public void Clear() => _uncommittedEvents.Clear();

        public void ReplayEvent<TEvent>(TEvent evt) where TEvent : IEvent
        {
            var eventHandler = this as IEventHandler<TEvent>;
            if (eventHandler != null && this.Version + 1 == evt.Version)
            {
                eventHandler.Handle(evt);
                this.Version = evt.Version;
            }
        }

        protected void RaiseEvent<TEvent>(TEvent evt) where TEvent : IEvent
        {
            var eventHandler = this as IEventHandler<TEvent>;
            evt.Version = this.Version + 1;
            this.Version = evt.Version;
            if (eventHandler != null)
            {
                eventHandler.Handle(evt);
            }
            _uncommittedEvents.Enqueue(evt);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Rainbow.DomainDriven.Core.Extensions;

namespace Rainbow.DomainDriven.Event
{
    public abstract class DomainEvent : IEvent
    {
        public DomainEvent()
            : this(EventOperation.Updated)
        {
        }
        public DomainEvent(EventOperation operation)
            : this(DateTime.Now.ToSequenceGuid(), operation)
        {

        }
        public DomainEvent(Guid id, EventOperation operation)
        {
            this.UTCTimestamp = DateTime.Now.ToUniversalTime().Ticks;
            this.Id = id;
            this.Operation = operation;
        }

        public long UTCTimestamp { get; protected set; }
        public int Version { get; set; }
        public Guid Id { get; protected set; }

        public Guid AggregateRootId { get; set; }

        public string AggregateRootTypeName { get; set; }
        public EventOperation Operation { get; set; }
    }
}

using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public interface IRollbackService
    {
        IEnumerable<IAggregateRoot> Redo(IEnumerable<IAggregateRoot> roots);
    }
}
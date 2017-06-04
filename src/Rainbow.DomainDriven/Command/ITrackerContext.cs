using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Command
{
    public interface ITrackerContext
    {
        IEnumerable<IAggregateRoot> TrackedAggregateRoots { get; }
        void Clear();
    }
}

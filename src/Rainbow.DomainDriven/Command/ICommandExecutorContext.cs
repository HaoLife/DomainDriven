using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandExecutorContext : ICommandContext
    {
        IEnumerable<IAggregateRoot> TrackedAggregateRoots { get; }
        void Clear();
    }
}
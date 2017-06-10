using System.Collections.Generic;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventExecutor
    {
        void Handle(params EventSource[] sources);
    }
}
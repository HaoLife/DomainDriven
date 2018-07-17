using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Store
{
    public interface ISubscribeEventStore
    {
        SubscribeEvent Get(Guid id);
        void Save(SubscribeEvent subscribeEvent);
    }
}

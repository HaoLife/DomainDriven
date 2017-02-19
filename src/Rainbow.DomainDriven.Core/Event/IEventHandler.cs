using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandler<in TEvent>
    {
        void Handle(TEvent evt);
        
        //Task HandleAsync(TEvent evt);
    }
}

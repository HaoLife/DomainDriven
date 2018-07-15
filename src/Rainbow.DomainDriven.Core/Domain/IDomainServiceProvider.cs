using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Domain
{
    public interface IDomainServiceProvider
    {
        TDomainService Get<TDomainService>() where TDomainService : IDomainService;
    }
}

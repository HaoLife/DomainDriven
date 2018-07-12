using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Domain
{
    public interface IEntity
    {
        Guid Id { get; }
    }
}

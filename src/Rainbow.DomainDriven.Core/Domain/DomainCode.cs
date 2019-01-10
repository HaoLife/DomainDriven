using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Domain
{
    public enum DomainCode
    {
        AggregateExists = 0x01,
        CommandHandlerNotExists = 0x02,
        CommandHandlerCannotCreate = 0x03,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Message
{
    public enum ConsistencyLevel
    {
        Lose = 0,
        Finally,
        Strong,
    }
}

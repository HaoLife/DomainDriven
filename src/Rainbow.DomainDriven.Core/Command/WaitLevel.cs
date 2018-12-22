using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public enum WaitLevel
    {
        NotWait = -1,
        Handle = 0,
        Snapshot = 1,
        Business = 2,
    }
}

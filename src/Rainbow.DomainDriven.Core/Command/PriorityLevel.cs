using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    /// <summary>
    /// 优先级
    /// </summary>
    public enum PriorityLevel
    {
        Normal = 0,
        High = 1,
        Low = -1,
    }
}

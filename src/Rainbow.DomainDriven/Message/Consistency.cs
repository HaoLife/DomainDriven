using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Message
{
    /// <summary>
    /// 一致性
    /// </summary>
    public enum Consistency
    {
        Lose = 0,
        Finally,
        Strong,
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Framework
{
    public interface IDatabaseInitializer
    {
        void Initialize();
        bool IsRun { get; }
    }
}

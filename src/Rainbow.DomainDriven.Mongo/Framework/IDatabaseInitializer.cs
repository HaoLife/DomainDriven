using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Mongo.Framework
{
    public interface IDatabaseInitializer
    {
        void Initialize();
        bool IsRun { get; }
    }
}

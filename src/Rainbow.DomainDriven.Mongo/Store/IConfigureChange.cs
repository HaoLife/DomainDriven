using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Mongo.Store
{
    public interface IConfigureChange
    {
        void Reload();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandMappingProvider
    {
        Dictionary<Guid, Type> Find(ICommand cmd);

    }
}

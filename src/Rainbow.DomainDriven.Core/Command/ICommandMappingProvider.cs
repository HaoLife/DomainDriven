using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandMappingProvider
    {
        IEnumerable<KeyValuePair<Guid, Type>> Find(ICommand cmd);

    }
}

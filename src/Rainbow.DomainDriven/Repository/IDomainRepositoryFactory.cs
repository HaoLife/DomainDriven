using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Repository
{
    public interface IDomainRepositoryFactory
    {
        TRepository Create<TRepository>() where TRepository : class, IDomainRepository;
    }
}
